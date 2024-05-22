using System.Globalization;
using DG.Tweening;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System; 
namespace Project.Spells
{
    public class SpellUI : MonoBehaviour
    {
        private CooldownController _cooldowns;
        [SerializeField] private int id;

        [SerializeField] private CanvasGroup group;
        [SerializeField] private TextMeshProUGUI tmp;
        [SerializeField] private Image cooldownFilter;
        [SerializeField] private Image spellIcon; 

        private float _maxTime;
        
        private void Awake()
        {
            UserInstance.Me.OnPlayerLinked += Setup;
        }

        private void OnDestroy()
        {
            if (UserInstance.Me != null) UserInstance.Me.OnPlayerLinked -= Setup;
            
            if (!_cooldowns) return;
            
            // For clients only
            _cooldowns.OnLocalCooldownStarted -= OnCooldownStarted;
            _cooldowns.OnLocalCooldownUpdated -= OnCooldownUpdated;
            
            // For host only
            _cooldowns.OnServerCooldownStarted -= OnCooldownStarted;
            _cooldowns.OnServerCooldownUpdated -= OnCooldownUpdated;
            
            _cooldowns.OnServerCooldownEnded -= OnCooldownEnded;
        }

        private void Setup(PlayerRefs refs)
        {
            _cooldowns = refs.Cooldowns;

            if (!NetworkManager.Singleton.IsHost)
            {
                _cooldowns.OnLocalCooldownStarted += OnCooldownStarted;
                _cooldowns.OnLocalCooldownUpdated += OnCooldownUpdated;
            }
            else
            {
                _cooldowns.OnServerCooldownStarted += OnCooldownStarted;
                _cooldowns.OnServerCooldownUpdated += OnCooldownUpdated;
            }
            
            _cooldowns.OnServerCooldownEnded += OnCooldownEnded;

            group.alpha = 0;

            try
            {
                PCPlayerRefs PCplayer = (PCPlayerRefs)refs;
                PlayerController playerController = (PlayerController)PCplayer.Entity;
                void SetUpSprite()
                {
                    SOCharacter character = (SOCharacter)playerController.data;
                    SpellData spellData;
                    if (character.TryGetSpell(id, out spellData)) spellIcon.sprite = spellData.spellIcon;
                    playerController.onEntityInit -= SetUpSprite;
                }
                playerController.onEntityInit += SetUpSprite; 
            }
            catch(Exception e)
            {
                Debug.LogError("rien à foutre");
            }
        }
        
        private void OnCooldownStarted(int index, float time)
        {
            if (index != id) return;
            
            group.DOFade(1, 0);
            cooldownFilter.fillAmount = 1;
            tmp.text = Mathf.RoundToInt(time).ToString();

            _maxTime = time;
        }

        private void OnCooldownUpdated(int index, float value)
        {
            if (index != id) return;
            
            tmp.text = value.ToString(CultureInfo.InvariantCulture);
            cooldownFilter.fillAmount = value / _maxTime;
        }
        
        private void OnCooldownEnded(int index)
        {
            if (index != id) return;
            
            group.DOFade(0, 0);
        }
    }
}