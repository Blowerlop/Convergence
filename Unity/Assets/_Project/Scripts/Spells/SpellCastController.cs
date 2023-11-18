using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project._Project.Scripts.Spells
{
    public class SpellCastController : MonoBehaviour
    {
        [SerializeField] private Transform playerTransform;
        
        private const int SpellsCount = 4;
        
        [SerializeField] private SpellCastersList spellCastersList;
        
        [PropertySpace(25)]
        
        [RequiredListLength(SpellsCount), SerializeField] private SpellData[] spells = new SpellData[SpellsCount];

        private readonly SpellCaster[] _spellCasters = new SpellCaster[SpellsCount];
        
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
                _spellCasters[i].Init(playerTransform);
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
            
            if (!_spellCasters[spellIndex].IsChanneling) return;

            _currentChannelingIndex = null;
            
            _spellCasters[spellIndex].StopChanneling();
            var results = _spellCasters[spellIndex].GetResults();
            
            Debug.LogError(results.ToString());
            
            //Ask SpellManager to spawn the according spell with the given results
        }
    }
}