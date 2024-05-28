using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Project
{
    public class HealthText : MonoBehaviour
    {
        [SerializeField] private float jumpPower;
        [SerializeField] private float duration;

        [SerializeField] private float distance;

        [SerializeField] private TextMeshPro text;

        [Button]
        public void Init(int value, Vector3 position, Vector3 direction)
        {
            transform.position = position;

            var target = position + direction * distance;
            
            transform.DOJump(target, jumpPower, 1, duration);
            
            text.text = value.ToString();
        }
    }
}
