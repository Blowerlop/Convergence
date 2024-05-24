using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project
{
    public class PlayerSpellsAnimFXHandler : SerializedMonoBehaviour
    {
        [Serializable]
        private class Handler
        {
            [field: SerializeField] public string StateName { get; private set; }
            
            [field: SerializeField, PropertySpace(5, 20)]
            public List<FXWrapper> FXs { get; private set; } = new();

            private List<ParticleSystem> _nextAutoFxs = new();
            
            public void OnStateEnter()
            {
                foreach (var wrapper in FXs)
                {
                    wrapper.FX.Play();

                    switch (wrapper.DisableType)
                    {
                        case FXDisableType.Timed:
                            DOVirtual.DelayedCall(wrapper.DisableTime, wrapper.FX.Stop);
                            break;
                        case FXDisableType.OnNextAutoAttack:
                            _nextAutoFxs.Add(wrapper.FX);
                            break;
                    }
                }
            }
            
            public void OnStateExit()
            {
                foreach (var wrapper in FXs)
                {
                    if(wrapper.DisableType == FXDisableType.OnStateExit)
                    {
                        wrapper.FX.Stop();
                    }
                }
            }
            
            public void OnAutoAttack()
            {
                foreach (var fx in _nextAutoFxs)
                {
                    fx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
                
                _nextAutoFxs.Clear();
            }
        }

        [Serializable]
        private class FXWrapper
        {
            [field: SerializeField] public ParticleSystem FX { get; private set; }
            
            [field: SerializeField] public FXDisableType DisableType { get; private set; }
            
            [field: SerializeField, ShowIf(nameof(IsTimed))] public float DisableTime { get; private set; }
            
            private bool IsTimed => DisableType == FXDisableType.Timed;
        }
        
        private enum FXDisableType
        {
            OnStateExit,
            Timed,
            OnNextAutoAttack
        }
        
        private Dictionary<int, Handler> _handlers = new();

        [SerializeField] private PCPlayerRefs _playerRefs;
        
        [SerializeField, ListDrawerSettings(ShowFoldout = false, ShowIndexLabels = false)] private List<Handler> handlers = new();

        private void Awake()
        {
            foreach (var handler in handlers)
            {
                _handlers.Add(Animator.StringToHash(handler.StateName), handler);
            }
            
            _playerRefs.AttackController.OnHit += OnAutoAttack;
        }

        private void OnDestroy()
        {
            _playerRefs.AttackController.OnHit -= OnAutoAttack;
        }

        public void OnStateEnter(int fullHash)
        {
            if (!_handlers.ContainsKey(fullHash))
            {
                return;
            }
            
            _handlers[fullHash].OnStateEnter();
        }
        
        public void OnStateExit(int fullHash)
        {
            if (!_handlers.ContainsKey(fullHash))
            {
                return;
            }
            
            _handlers[fullHash].OnStateExit();
        }
        
        public void OnAutoAttack()
        {
            foreach (var handler in _handlers)
            {
                handler.Value.OnAutoAttack();
            }
        }
    }
}