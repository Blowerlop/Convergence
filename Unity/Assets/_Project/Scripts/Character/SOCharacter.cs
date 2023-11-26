using System.Collections.Generic;
using System.Text;
using Project.Extensions;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Project
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Character")]
    public class SOCharacter : ScriptableObject
    {
        [ShowInInspector, PropertyOrder(-1)] public int id = -1;

        [field: SerializeField, AssetsOnly, Required]
        [field: OnValueChanged("RenameAssetByName", InvokeOnInitialize = true, InvokeOnUndoRedo = true)]
        [field: OnValueChanged("SetId", InvokeOnInitialize = true,  InvokeOnUndoRedo = true)]
        public string characterName { get; private set; }
        [field: SerializeField, AssetsOnly, Required, PreviewField(75)] public Sprite avatar { get; private set; }
        [field: SerializeField, AssetsOnly, Required] public GameObject prefab { get; private set; }
        [field: SerializeField, AssetsOnly, Required] public GameObject model { get; private set; }

        // Stats

        private void RenameAssetByName()
        {
            string path = AssetDatabase.GetAssetPath(GetInstanceID());
            AssetDatabase.RenameAsset(path, characterName);
        }

        private void SetId()
        {
            id = characterName.ToHashIsSameAlgoOnUnreal();
        }

        private static IEnumerable<SOCharacter> GetAllCharacters()
        {
            return Utilities.FindAssetsByType<SOCharacter>();;
        }

        public static bool TryGetCharacter(int id, out SOCharacter characterData)
        {
            characterData = GetCharacter(id);
            return characterData != null;
        }

        public static SOCharacter GetCharacter(int id)
        {
            IEnumerable<SOCharacter> characters = GetAllCharacters();
            foreach (var character in characters)
            {
                if (character.id == id) return character;
            }

            return null;
        }

        [Button]
        private void FindIfCharactersHaveTheSameId()
        {
            Debug.Log("Start searching...");

            IEnumerable<SOCharacter> characters = GetAllCharacters();

            Dictionary<int, List<string>> ids = new Dictionary<int, List<string>>();
            foreach (SOCharacter character in characters)
            {
                if (ids.ContainsKey(character.id) == false)
                {
                    ids.Add(character.id, new List<string>());
                }
                
                ids[character.id].Add(character.name);
                
            }

            StringBuilder stringBuilder = new StringBuilder();
            int duplicatedIds = 0;
            
            foreach (var kvp in ids)
            {
                if (kvp.Value.Count == 1) continue;
                duplicatedIds++;
                stringBuilder.AppendLine(kvp.Key + ":");
                
                foreach (string value in kvp.Value)
                {
                    stringBuilder.AppendLine("- " + value);
                }

                stringBuilder.AppendLine();
            }
            
            Debug.Log($"Search ended ! {(duplicatedIds == 0 ? "No duplicated id." : $"{duplicatedIds} duplicated id:" + "\n" + stringBuilder.ToString())}");
        }
        
        [Button]
        [Tooltip("Sometimes Github don't detect the changes made on the ScriptableObject, so we need to force the write on the disk")]
        private void ForceSaveOnDisk()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    // I've not been able to override all ScriptableObject inspectors to display the button 
    
    // #if UNITY_EDITOR
    // [CustomEditor(typeof(ScriptableObject), true)]
    // public class ScriptableObjectEditor : OdinEditor
    // {
    //     public override void OnInspectorGUI()
    //     {
    //         base.OnInspectorGUI();
    //
    //         // SOCharacter t = target as SOCharacter;
    //         // if (t == null) return;
    //         Debug.Log("Working");
    //         
    //         if (GUILayout.Button("Force save on disk"))
    //         {
    //             EditorUtility.SetDirty(target);
    //             AssetDatabase.SaveAssets();
    //             AssetDatabase.Refresh();
    //         }
    //     }
    // }
    // #endif
}
