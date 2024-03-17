using Sirenix.OdinInspector;
using UnityEngine;

namespace Project._Project.Scripts
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Projectile")]
    public class SOProjectile : ScriptableObject
    {
        [field: SerializeField, AssetsOnly, PreviewField] public GameObject modelPrefab { get; private set; }
        [SerializeField] public float _speed = 1.0f;
    }
}