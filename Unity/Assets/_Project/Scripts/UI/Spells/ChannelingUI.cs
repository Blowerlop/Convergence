using DG.Tweening;
using Project._Project.Scripts.Spells;
using UnityEngine;

namespace Project.Spells
{
    public class ChannelingUI : MonoBehaviour
    {
        private ChannelingController _channelingController;

        [SerializeField] private CanvasGroup group;
        
        private void Awake()
        {
            PlayerRefs.OnLocalPlayerSpawned += Setup;
            
            group.alpha = 0;
        }
        
        private void OnDestroy()
        {
            PlayerRefs.OnLocalPlayerSpawned -= Setup;
            
            if (!_channelingController) return;
            
            _channelingController.OnServerChannelingStarted -= OnChannelingStarted;
            _channelingController.OnServerChannelingEnded -= OnChannelingEnded;
        }
        
        private void Setup(PlayerRefs refs)
        {
            _channelingController = refs.GetChannelingController(PlayerPlatform.Pc);
            
            _channelingController.OnServerChannelingStarted += OnChannelingStarted;
            _channelingController.OnServerChannelingEnded += OnChannelingEnded;
        }
        
        private void OnChannelingStarted()
        {
            group.DOFade(1, 0.15f);
        }
        
        private void OnChannelingEnded()
        {
            group.DOFade(0, 0.15f);
        }
    }
}