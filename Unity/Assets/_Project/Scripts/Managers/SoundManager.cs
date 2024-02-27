using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using Sirenix.Utilities;
using UnityEngine;
using Debug = UnityEngine.Debug;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace Project._Project.Scripts.Managers
{
    public class SoundInstance
    {
        public string EventName;
        public SoundManager.EventType EventType;
        public EventInstance EventInstance;
        public SoundInstance(string eventName, SoundManager.EventType eventType, EventInstance eventInstance)
        {
            EventName = eventName;
            EventType = eventType;
            EventInstance = eventInstance;
        }
    }

    public class PlayerAction
    {
        public UserInstance Player;
        public int Priority;
        public float LifeTime;
        public float CreatedTime;
        public string ActionName;
        public bool CutLowerPriority;
    }
    
    public class SoundManager : MonoSingleton<SoundManager>
    {
        private List<string> _eventsPath;

        private readonly Dictionary<string, SoundInstance> _staticSoundList = new Dictionary<string, SoundInstance>();
        private readonly Dictionary<string, SoundInstance> _globalSoundList = new Dictionary<string, SoundInstance>();
        private readonly Dictionary<string, SoundInstance> _snapshotList = new Dictionary<string, SoundInstance>();
        private Dictionary<int, PlayerAction> _actionPool;

        private IEnumerator _poolCoroutine;

        public enum EventType
        {
            Music,
            Ambiance,
            PlayerAction,
            SFX,
            Snapshot,
        }

        protected override void Awake()
        {
            dontDestroyOnLoad = false;
            base.Awake();
        }

        void Start()
        {
            _poolCoroutine = ConsumePoolCoroutine();

            StartCoroutine(_poolCoroutine);
        }

        #region Sound
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventAlias"></param>
        /// <param name="type"></param>
        public void PlayGlobalSound(string eventName, string eventAlias, EventType type)
        {
            if (RuntimeManager.StudioSystem.getEvent(eventName, out var eventInfo) != RESULT.OK)
                goto errCallback;

            if (_globalSoundList.TryGetValue(eventAlias, out var value))
            {
                value.EventInstance.getPlaybackState(out var state);
                if(state == PLAYBACK_STATE.PLAYING)
                    value.EventInstance.stop(STOP_MODE.ALLOWFADEOUT);
                _globalSoundList.Remove(eventAlias);
            }

            eventInfo.createInstance(out var eventInstance);
            
            if(eventInstance.start() != RESULT.OK)
                goto errCallback;
            
            _globalSoundList.Add(eventAlias, new SoundInstance(eventName, type, eventInstance));
            
            return;
            
            errCallback:
            Debug.LogWarning("Fmod: Event:" + eventName + " does not exist prévenir Guillaume");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventAlias"></param>
        public void StopGlobalSound(string eventAlias)
        {
            if (_staticSoundList.TryGetValue(eventAlias, out var value))
            {
                value.EventInstance.getPlaybackState(out var state);
                if(state == PLAYBACK_STATE.PLAYING)
                    value.EventInstance.stop(STOP_MODE.ALLOWFADEOUT);
                _staticSoundList.Remove(eventAlias);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventAlias"></param>
        /// <param name="position"></param>
        /// <param name="type"></param>
        public void PlayStaticSound(string eventName, string eventAlias, Vector3 position, EventType type)
        {
            if (RuntimeManager.StudioSystem.getEvent(eventName, out var eventInfo) != RESULT.OK)
                goto errCallback;

            if (_staticSoundList.TryGetValue(eventAlias, out var value))
            {
                value.EventInstance.getPlaybackState(out var state);
                if(state == PLAYBACK_STATE.PLAYING)
                    value.EventInstance.stop(STOP_MODE.ALLOWFADEOUT);
                _staticSoundList.Remove(eventAlias);
            }

            eventInfo.createInstance(out var eventInstance);
            
            eventInstance.set3DAttributes(position.To3DAttributes());
            
            if(eventInstance.start() != RESULT.OK)
                goto errCallback;
            
            _staticSoundList.Add(eventAlias, new SoundInstance(eventName, type, eventInstance));
            
            return;
            
            errCallback:
            Debug.LogWarning("Fmod: Event:" + eventName + " does not exist prévenir Guillaume");
        }
        
        public void PlayStaticSound(string eventName, string eventAlias, GameObject target, EventType type)
        {
            if (RuntimeManager.StudioSystem.getEvent(eventName, out var eventInfo) != RESULT.OK)
                goto errCallback;

            if (_staticSoundList.TryGetValue(eventAlias, out var value))
            {
                value.EventInstance.getPlaybackState(out var state);
                if(state == PLAYBACK_STATE.PLAYING)
                    value.EventInstance.stop(STOP_MODE.ALLOWFADEOUT);
                _staticSoundList.Remove(eventAlias);
            }
            
            eventInfo.createInstance(out var eventInstance);
            
            RuntimeManager.AttachInstanceToGameObject(eventInstance, target.transform);
            
            if(eventInstance.start() != RESULT.OK)
                goto errCallback;
            
            _staticSoundList.Add(eventAlias, new SoundInstance(eventName, type, eventInstance));
            
            errCallback:
            Debug.LogWarning("Fmod: Event:" + eventName + " does not exist prévenir Guillaume");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventAlias"></param>
        public void StopStaticSound(string eventAlias)
        {
            if (_staticSoundList.TryGetValue(eventAlias, out var value))
            {
                value.EventInstance.getPlaybackState(out var state);
                if(state == PLAYBACK_STATE.PLAYING)
                    value.EventInstance.stop(STOP_MODE.ALLOWFADEOUT);
                _staticSoundList.Remove(eventAlias);
            }
        }

        public void AddPlayerActionSound(string actionName, UserInstance player)
        {
            //TODO
        }

        public void ClearPlayerActionPool(UserInstance player)
        {
            //TODO
        }

        public void ClearAllPlayerActionPool()
        {
            //TODO
        }

        public IEnumerator ConsumePoolCoroutine()
        {
            yield return null; //TODO
        }

        #endregion

        #region Utils

        public void TriggerSnapshot(string snapshotName, string snapshotAlias)
        {
            if (RuntimeManager.StudioSystem.getEvent("snapshot:/" + snapshotName, out var eventInfo) != RESULT.OK)
                goto errCallback;

            if (_snapshotList.TryGetValue(snapshotAlias, out var value))
            {
                value.EventInstance.getPlaybackState(out var state);
                if(state == PLAYBACK_STATE.PLAYING)
                    value.EventInstance.stop(STOP_MODE.ALLOWFADEOUT);
                _snapshotList.Remove(snapshotAlias);
            }
            
            eventInfo.createInstance(out var eventInstance);
            
            if(eventInstance.start() != RESULT.OK)
                goto errCallback;

            _snapshotList.Add(snapshotAlias,
                new SoundInstance("snapshot:/" + snapshotName, EventType.Snapshot, eventInstance));
            
            return;
            
            errCallback:
            Debug.LogWarning("Fmod: Event: snapshot:/" + snapshotName + " does not exist prévenir Guillaume");
        }
        
        public void StopSnapshot(string snapshotAlias)
        {
            if (_snapshotList.TryGetValue(snapshotAlias, out var value))
            {
                value.EventInstance.getPlaybackState(out var state);
                if(state == PLAYBACK_STATE.PLAYING)
                    value.EventInstance.stop(STOP_MODE.ALLOWFADEOUT);
                _snapshotList.Remove(snapshotAlias);
                return;
            }
            
            Debug.LogWarning("Fmod: Event: " + snapshotAlias + " does not exist prévenir Guillaume");
        }
        
        public List<string> GetAllEvent()
        {
            if (_eventsPath != null) return _eventsPath;

            _eventsPath = new List<string>();
            RuntimeManager.StudioSystem.getBankList(out Bank[] loadedBanks);

            foreach (var bank in loadedBanks)
            {
                bank.getEventList(out var events);
                events.ForEach(x => x.getPath(out var path));
                foreach (var eventRef in events)
                {
                    eventRef.getPath(out var path);
                    _eventsPath.Add(path);
                }
            }

            return _eventsPath;
        }

        #endregion
    }
}
