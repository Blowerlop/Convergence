using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Spells.Casters
{
    public class SpellCastController : MonoBehaviour
    {
        [SerializeField] private SOScriptableObjectReferencesCache _soScriptableObjectReferencesCache;
        
        private PlayerRefs _player;
        private CooldownController _cooldowns;
        
        private SpellData[] _spells;
        private SpellCaster[] _spellCasters;
        
        private int? _currentChannelingIndex;
        
        public void Init(PlayerRefs p)
        {
            _player = p;
            
            // Pc user should be player owner
            if (!_player.IsOwner)
            {
                // Can cast spell only from local player
                Destroy(gameObject);
                return;
            }

            _cooldowns = _player.GetCooldownController(PlayerPlatform.Pc);

            if (!InitSpells()) return;
            InitSpellCasters();
            InitInputs();
        }
        
        private void OnDestroy()
        {
            if (InputManager.isBeingDestroyed || InputManager.instance == null) return;
            
            InputManager.instance.OnSpellInputStarted -= StartChanneling;
            InputManager.instance.OnOnSpellInputCanceled -= StopChanneling;
            InputManager.instance.onMouseButton0.started -= StopChanneling;
        }

        private void InitInputs()
        {
            InputManager.instance.OnSpellInputStarted += StartChanneling;
            InputManager.instance.OnOnSpellInputCanceled += StopChanneling;
            InputManager.instance.onMouseButton0.started += StopChanneling;
        }
        
        private bool InitSpells()
        {
            SOCharacter.TryGetCharacter(_soScriptableObjectReferencesCache, UserInstance.Me.CharacterId, out var character);
            if(character == null)
            {
                Debug.LogError($"SpellCastController > Can't InitSpells because character {UserInstance.Me.CharacterId} can't be found.");
                return false;
            }

            _spells = character.GetSpells();
            return true;
        }
        
        private void InitSpellCasters()
        {
            _spellCasters = new SpellCaster[_spells.Length];
            
            for (int i = 0; i < _spells.Length; i++)
            {
                var spellData = _spells[i];

                var prefab = spellData.requiredCaster;

                if (prefab == null)
                {
                    Debug.LogError($"SpellCaster of spell {spellData.spellId} is null!");
                    continue;
                }
                
                if(prefab.CastResultType != spellData.requiredResultType)
                {
                    Debug.LogError($"Spell {spellData.spellId}'s requiredResultType and SpellCaster prefab's ChannelingResultType are different!\n" +
                                   $"Maybe you forgot to set the requiredResultType or you picked a SpellCaster prefab that doesn't return the right type.");
                    continue;
                }

                var ownTransform = transform;
                _spellCasters[i] = Instantiate(prefab, ownTransform.position, Quaternion.identity, ownTransform);
                _spellCasters[i].Init(_player.PlayerTransform, _spells[i]);
            }
        }

        private void StartChanneling(int spellIndex)
        {
            if (spellIndex < 0 || spellIndex >= _spells.Length)
            {
                Debug.LogError($"Spell index {spellIndex} is out of range.");
                return;
            }
            
            if (_spellCasters.Any(x => x.IsChanneling)) return;

            if (_cooldowns.IsInCooldown(spellIndex)) return;
            
            _currentChannelingIndex = spellIndex;
            _spellCasters[spellIndex].StartChanneling();
            
            //If the spell is instant, get the results right away
                //var results = _spellCasters[spellIndex].GetResults();
                //Ask SpellManager to spawn the according spell with the results
        }

        private void StopChanneling(InputAction.CallbackContext _)
        {
            if (!_currentChannelingIndex.HasValue) return;
            
            StopChanneling(_currentChannelingIndex.Value);
        }
        
        private void StopChanneling(int spellIndex)
        {
            if(spellIndex < 0 || spellIndex >= _spells.Length)            
            {
                Debug.LogError($"Spell index {spellIndex} is out of range.");
                return;
            }
            
            var caster = _spellCasters[spellIndex];
            
            if (!caster.IsChanneling) return;

            _currentChannelingIndex = null;
            
            caster.StopChanneling();
            caster.EvaluateResults();
            
            caster.TryCast(spellIndex);
            
            _cooldowns.StartLocalCooldown(spellIndex, _spells[spellIndex].cooldown);
        }
    }
}