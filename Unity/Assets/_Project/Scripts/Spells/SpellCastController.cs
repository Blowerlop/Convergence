using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Spells.Casters
{
    public class SpellCastController : MonoBehaviour
    {
        // TODO: Gather this ref from a player controller when there will be one
        [SerializeField] private CooldownController cooldowns;
        
        [SerializeField] private Transform playerTransform;
        
        [SerializeField] private SpellCastersList spellCastersList;

        [PropertySpace(25)]
        
        [RequiredListLength(SpellData.CharacterSpellsCount), SerializeField]
        private SpellData[] spells = new SpellData[SpellData.CharacterSpellsCount];

        private readonly SpellCaster[] _spellCasters = new SpellCaster[SpellData.CharacterSpellsCount];
        
        private int? _currentChannelingIndex;
        
        private void Start()
        {
            InitSpellCasters();
            
            InputManager.instance.OnSpellInputStarted += StartChanneling;
            InputManager.instance.OnOnSpellInputCanceled += StopChanneling;
            InputManager.instance.onMouseButton0.started += StopChanneling;
        }

        private void OnDestroy()
        {
            if (InputManager.isBeingDestroyed || InputManager.instance == null) return;
            
            InputManager.instance.OnSpellInputStarted -= StartChanneling;
            InputManager.instance.OnOnSpellInputCanceled -= StopChanneling;
            InputManager.instance.onMouseButton0.started -= StopChanneling;
        }

        private void InitSpellCasters()
        {
            for (int i = 0; i < spells.Length; i++)
            {
                var spellData = spells[i];

                var prefab = spellCastersList.Get(spellData.castingType);

                if (prefab == null)
                {
                    Debug.LogError($"Spell {spellData.spellId} require a SpellCaster of type {spellData.castingType} " +
                                   $"but it can't be found. Please add one to the SpellCastersList.");
                    continue;
                }

                _spellCasters[i] = Instantiate(prefab, transform.position, Quaternion.identity, transform);
                _spellCasters[i].Init(playerTransform, spells[i]);
            }
        }

        private void StartChanneling(int spellIndex)
        {
            if (spellIndex < 0 || spellIndex >= spells.Length)
            {
                Debug.LogError($"Spell index {spellIndex} is out of range.");
                return;
            }
            
            if (_spellCasters.Any(x => x.IsChanneling)) return;

            if (cooldowns.IsInCooldown(spellIndex)) return;
            
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
            if(spellIndex < 0 || spellIndex >= spells.Length)            
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
            
            cooldowns.StartLocalCooldown(spellIndex, spells[spellIndex].cooldown);
        }

        public SpellData GetSpellAtIndex(int index)
        {
            return spells[index];
        }
    }
}