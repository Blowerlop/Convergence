using Cysharp.Threading.Tasks;
using FMODUnity;
using Project._Project.Scripts;
using Project.Spells.Casters;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;

namespace Project
{
    public class TutorialSequencer : MonoBehaviour
    {

        #region Public
        public GameObject MobilePlayer; 
        public Transform tutorialMvtCheck;
        public StudioEventEmitter[] fmodEmitterList; 
        #endregion


        #region Privates

        [SerializeField] SOCharacter tutorialCharacter;
        CancellationTokenSource cts;
        UserInstance userInstance;
        PCPlayerRefs PcRefs;
        [SerializeField] Dummy tutorialDummy;

        const int timeOutDelay = 5000; 
        #endregion

       


        private void Start()
        {
            _ = PlayTutorial();
        }
        bool testc;

        private void Update()
        {
            if(testc)
            {
                
            }
        }
        async UniTask PlayTutorial()
        {
            await LoadTutorialScene();

            //Intro
            await PlaySoundAsync(fmodEmitterList[0]);

            await CheckMovementSequence();

            await CheckAttackSequence();

            await CheckSpellSequence();

            await PlaySoundAsync(fmodEmitterList[5]);

            await PlaySoundAsync(fmodEmitterList[6]);
        }


        #region Tuto Spell

        async UniTask CheckSpellSequence()
        {
            await PlaySoundAsync(fmodEmitterList[3]);

            SpellCastController temp = PcRefs.gameObject.AddComponent<SpellCastController>();
            temp.Init(PcRefs);
            Debug.Log("<color=Yellow> Spell OK </color>");
        }

        #endregion

        #region Tuto Attack

        async UniTask CheckAttackSequence()
        {
            await PlaySoundAsync(fmodEmitterList[2]);

            SpawnDummy();

            await UniTask.WaitUntil(() => userInstance.LinkedPlayer.Animator.GetCurrentAnimatorStateInfo(0).fullPathHash == -1146545271);


            Debug.Log("<color=Yellow> Attack OK </color>");
        }

        #endregion

        #region Tuto Movement

        async UniTask CheckMovementSequence()
        {
            await PlaySoundAsync(fmodEmitterList[1]);

            tutorialMvtCheck.gameObject.SetActive(true);

            await UniTask.WaitUntil(() => (userInstance.LinkedPlayer.transform.position - tutorialMvtCheck.position).sqrMagnitude < 5f);
            
            Debug.Log("<color=Yellow> Movement OK </color>");

            tutorialMvtCheck.gameObject.SetActive(false);
        }

        #endregion

        #region Loading Scene
        async UniTask LoadTutorialScene()
        {
            try
            {
                StartHost();

                cts = new CancellationTokenSource(timeOutDelay);
                await UniTask.WaitUntil(() => NetworkManager.Singleton.IsServer, PlayerLoopTiming.Update, cts.Token);

                cts.Dispose();
                cts = new CancellationTokenSource(timeOutDelay);
                await UniTask.WaitUntil(() => TeamManager.instance.TrySetTeam((int)NetworkManager.Singleton.LocalClientId, 0, PlayerPlatform.Pc), PlayerLoopTiming.Update, cts.Token);

                cts.Dispose();
                cts = new CancellationTokenSource(timeOutDelay);
                ValidateCharacterServerRpcc();


                await UniTask.WaitUntil(() => UserInstanceManager.instance.GetUserInstance((int)NetworkManager.Singleton.LocalClientId).CharacterId != 0, PlayerLoopTiming.Update, cts.Token);
                userInstance = UserInstanceManager.instance.GetUserInstance((int)NetworkManager.Singleton.LocalClientId);
                cts.Dispose();

                LoadScene();


                await UniTask.WaitUntil(() => userInstance != null && userInstance.LinkedPlayer != null, PlayerLoopTiming.Update);

                //Attach Mobile
                MobilePlayer.transform.SetParent(userInstance.LinkedPlayer.transform);
                MobilePlayer.transform.localPosition = new Vector3(1.5f, 2, 0);


                //Disable Spells
                PcRefs = (PCPlayerRefs)userInstance.LinkedPlayer;
                Destroy(PcRefs.SpellCastController);


                Debug.Log("<color=Yellow> Loading Tutorial OK </color>");
            }
            catch (Exception ex)
            {
                cts?.Dispose();
                Debug.Log("Error in tutorial set up, back to menu - " + ex.Message);
                SceneManager.Network_LoadSceneAsync("Menu", LoadSceneMode.Single, new LoadingScreenParameters(null, Color.black));
            }
        }
        void StartHost() => Netcode_ConnectionManager.StartHost();

        public void LoadScene()
        {
            SceneManager.Network_LoadSceneAsync("SpellAdditive", LoadSceneMode.Additive, new LoadingScreenParameters(null, Color.black));
        }

        public void ValidateCharacterServerRpcc() => ValidateCharacterServerRpc((int)NetworkManager.Singleton.LocalClientId, tutorialCharacter.id);


        [ServerRpc(RequireOwnership = false)]
        private void ValidateCharacterServerRpc(int clientId, int characterId)
        {
            UserInstance userInstance = UserInstanceManager.instance.GetUserInstance(clientId);
            if (userInstance.CharacterId == characterId) return;

            userInstance.SetCharacter(characterId);
        }

        void SpawnDummy()
        {
            tutorialDummy = Instantiate(tutorialDummy, new Vector3(5, 0, 5), Quaternion.identity);
            tutorialDummy.GetComponent<NetworkObject>().Spawn();
        }
        #endregion

        #region Utility

        async UniTask PlaySoundAsync(StudioEventEmitter eventEmitter)
        {
            eventEmitter.gameObject.SetActive(true);
            while (eventEmitter.IsPlaying())
            {
                await UniTask.Yield();
            }

            eventEmitter.gameObject.SetActive(false);
        }

        #endregion


    }
}
