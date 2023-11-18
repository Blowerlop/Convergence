using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project._Project.Scripts.Spells
{
    public class SpellCastController : MonoBehaviour
    {
        private const int SpellsCount = 4;
        
        [SerializeField] private SpellCastersList spellCastersList;
        
        [PropertySpace(25)]
        
        [RequiredListLength(SpellsCount), SerializeField] private SpellData[] spells = new SpellData[SpellsCount];

        private readonly SpellCaster[] _spellCasters = new SpellCaster[SpellsCount];
        
        private void Start()
        {
            InitSpellCasters();
            
            InputManager.instance.OnSpellInputStarted += StartChanneling;
            InputManager.instance.OnOnSpellInputCanceled += StopChanneling;
        }

        private void OnDestroy()
        {
            InputManager.instance.OnSpellInputStarted -= StartChanneling;
            InputManager.instance.OnOnSpellInputCanceled -= StopChanneling;
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

                _spellCasters[i] = Instantiate(prefab, transform);
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
            
            _spellCasters[spellIndex].StartChanneling();
            
            //If the spell is instant, get the results right away
                //var results = _spellCasters[spellIndex].GetResults();
                //Ask SpellManager to spawn the according spell with the results
        }
        
        private void StopChanneling(int spellIndex)
        {
            if(spellIndex < 0 || spellIndex >= spells.Length)            
            {
                Debug.LogError($"Spell index {spellIndex} is out of range.");
                return;
            }
            
            if (!_spellCasters[spellIndex].IsChanneling) return;
            
            _spellCasters[spellIndex].StopChanneling();
            var results = _spellCasters[spellIndex].GetResults();
            
            //Ask SpellManager to spawn the according spell with the given results
        }
    }
}