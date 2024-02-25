using System.Linq;
using Project._Project.Scripts.Spells;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Spells.Casters
{
    public class SpellCastController : MonoBehaviour
    {
        private PlayerRefs _player;
        private CooldownController _cooldowns;
        private ChannelingController _channelingController;
        
        private SpellData[] _spells;
        private SpellCaster[] _spellCasters;
        
        private int? _currentCastingIndex;

        private bool _needInit = true;
        
        public void Init(PlayerRefs p)
        {
            _player = p;
            
            // Pc user should be player owner
            if (_player.OwnerId != UserInstance.Me.ClientId)
            {
                // Can cast spell only from local player
                gameObject.SetActive(false);
                return;
            }
            
            if (!_needInit) return;
            _needInit = false;
            
            _cooldowns = _player.Cooldowns;
            _channelingController = _player.Channeling;

            if (!InitSpells()) return;
            InitSpellCasters();
            InitInputs();
        }
        
        private void OnDestroy()
        {
            if (InputManager.IsInstanceAlive() == false) return;
            
            InputManager.instance.OnSpellInputStarted -= StartCasting;
            InputManager.instance.OnOnSpellInputCanceled -= StopCasting;
            InputManager.instance.onMouseButton0.started -= StopCasting;
            InputManager.instance.onMouseButton1.started -= CancelCasting;
        }

        private void InitInputs()
        {
            InputManager.instance.OnSpellInputStarted += StartCasting;
            InputManager.instance.OnOnSpellInputCanceled += StopCasting;
            InputManager.instance.onMouseButton0.started += StopCasting;
            InputManager.instance.onMouseButton1.started += CancelCasting;
        }
        
        private bool InitSpells()
        {
            SOCharacter.TryGetCharacter(UserInstance.Me.CharacterId, out var character);
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

        private void StartCasting(int spellIndex)
        {
            if (spellIndex < 0 || spellIndex >= _spells.Length)
            {
                Debug.LogError($"Spell index {spellIndex} is out of range.");
                return;
            }
            
            if(_channelingController.IsChanneling) return;
            
            if (_spellCasters.Any(x => x.IsCasting)) return;
            
            if (_cooldowns.IsInCooldown(spellIndex)) return;
            
            _currentCastingIndex = spellIndex;
            _spellCasters[spellIndex].StartCasting();
            
            //If the spell is instant, get the results right away
                //var results = _spellCasters[spellIndex].GetResults();
                //Ask SpellManager to spawn the according spell with the results
        }

        private void StopCasting(InputAction.CallbackContext _)
        {
            if (!_currentCastingIndex.HasValue) return;
            
            StopCasting(_currentCastingIndex.Value);
        }
        
        private void StopCasting(int spellIndex)
        {
            if(spellIndex < 0 || spellIndex >= _spells.Length)            
            {
                Debug.LogError($"Spell index {spellIndex} is out of range.");
                return;
            }
            
            var caster = _spellCasters[spellIndex];

            if (!caster.CanStopCasting()) return;
            
            caster.StopCasting();
            
            caster.EvaluateResults();
            
            caster.TryCast(spellIndex);
            
            _cooldowns.StartLocalCooldown(spellIndex, _spells[spellIndex].cooldown);
            
            _currentCastingIndex = null;
        }
        
        private void CancelCasting(InputAction.CallbackContext _)
        {
            if (!_currentCastingIndex.HasValue) return;
            
            var caster = _spellCasters[_currentCastingIndex.Value];
            
            if (!caster.IsCasting) return;

            _currentCastingIndex = null;
            
            caster.StopCasting();
        }
    }
}