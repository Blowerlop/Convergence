using Cysharp.Threading.Tasks;
using Project._Project.Scripts;
using Project.Spells;
using Project.Spells.Casters;
using System;
using System.Threading;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project
{
    public class TutorialSequencer : MonoSingleton<MonoBehaviour>
    {

        #region Public
        public AudioHelper audioHelper; 
        public GameObject MobilePlayer;
        public GameObject coneGO; 
        public Transform tutorialMvtCheck;
        #endregion


        #region Privates

        [SerializeField] SOCharacter tutorialCharacter;
        CancellationTokenSource cts;
        UserInstance userInstance;
        PCPlayerRefs PcRefs;
        Canvas SpellCanvas; 
        [SerializeField] Dummy tutorialDummy;
        [SerializeField] TextMeshProUGUI subtitleTmp;

        const int timeOutDelay = 5000; 
        #endregion

        private void Start()
        {
            AudioHelper.OnTimestampReached += ReactToMessage;
            Subtitle.OnWrite += UpdateSubtitleText;
            _ = PlayTutorial();
        }

        private void OnDestroy()
        {
            AudioHelper.OnTimestampReached -= ReactToMessage;
            Subtitle.OnWrite += UpdateSubtitleText;
        }
        async UniTask PlayTutorial()
        {
            await LoadTutorialScene();

            //Intro
            Subtitle.StartWriting(null, "Welcome to Convergence ! I'm going to guide you through the game mechanics." +
                "This game is played in pairs, a PC player represented by the Ranger, and a Mobile Player represented by the Fairy." +
                "As a PC Player, you have the major role of controlling movements and attacks. The Mobile Player will act as a support," +
                " but you'll be in charge of all the moves.");
            await audioHelper.PlayAsync("Introduction", gameObject.GetCancellationTokenOnDestroy());
            

            await CheckMovementSequence();

            await CheckAttackSequence();

            await CheckSpellSequence();

            Subtitle.StartWriting(null, "Currently the camera is locked on your character by default. To lock or unlock your camera, double tap on your spacebar.");
            await audioHelper.PlayAsync("Camera", gameObject.GetCancellationTokenOnDestroy());

            await UniTask.Delay(1500);

            Subtitle.StartWriting(null, "A round is won when you manage to reduce your opponent's health point to 0. A game ends when a team has won two rounds.");

            await audioHelper.PlayAsync("WinCond", gameObject.GetCancellationTokenOnDestroy());

            await UniTask.Delay(1500);

            Subtitle.StartWriting(null, "You now have all the information you need to fight. This tutorial has only introduced you to one character, " +
                "don't hesitate to try the others and train your synergy with your mobile ally.");

            await audioHelper.PlayAsync("Conclusion", gameObject.GetCancellationTokenOnDestroy());
        }


        #region Tuto Spell

        async UniTask CheckSpellSequence()
        {
            Subtitle.StartWriting(null, "Each character has 4 unique spells. Try them out on the dummy as much as you like. Hover over the spells to" +
                " read informations on them.");
            await audioHelper.PlayAsync("Spell", gameObject.GetCancellationTokenOnDestroy());

            await UniTask.Delay(1500);

            Debug.Log("<color=Yellow> Spell OK </color>");
        }

        #endregion

        #region Tuto Attack

        async UniTask CheckAttackSequence()
        {
            Subtitle.StartWriting(null, "Great ! To attack, use your right mouse button while aiming at an opponent. Try to attack the dummy. ");
            await audioHelper.PlayAsync("Attack", gameObject.GetCancellationTokenOnDestroy());

            await UniTask.WaitUntil(() => userInstance.LinkedPlayer.Animator.GetCurrentAnimatorStateInfo(0).fullPathHash == -1146545271);

            coneGO.SetActive(false);

            await UniTask.Delay(1500);

            Debug.Log("<color=Yellow> Attack OK </color>");
        }

        void SpawnDummy()
        {
            tutorialDummy = Instantiate(tutorialDummy, new Vector3(5, 0, 5), Quaternion.identity);
            tutorialDummy.GetComponent<NetworkObject>().Spawn();

            coneGO.SetActive(true);
            coneGO.transform.SetParent(tutorialDummy.transform);
            coneGO.transform.localScale = 0.2f * Vector3.one;
            coneGO.transform.localPosition = new Vector3(0, 2f, 0);
        }
        #endregion

        #region Tuto Movement

        async UniTask CheckMovementSequence()
        {
            Subtitle.StartWriting(null, "To move, use the right click button. Try to move to the highlighted points. ");
            await audioHelper.PlayAsync("Movement", gameObject.GetCancellationTokenOnDestroy());

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
                SpellCanvas = FindObjectOfType<SpellUI>().transform.root.GetComponent<Canvas>() ;
                SpellCanvas.enabled = false;


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


        #endregion

        void UpdateSubtitleText(string currentPhrase)
        {
            subtitleTmp.text = currentPhrase;
        }

        void ReactToMessage(string message)
        {

            switch (message)
            {
                case "PCPlayerIndication":
                    coneGO.SetActive(true);
                    coneGO.transform.SetParent(PcRefs.transform);
                    coneGO.transform.localScale = 0.4f * Vector3.one;
                    coneGO.transform.localPosition = new Vector3(0, 5.49f, 0);
                    break;
                case "MobilePlayerIndication":
                    coneGO.transform.SetParent(MobilePlayer.transform);
                    coneGO.transform.localScale = 0.2f * Vector3.one;
                    coneGO.transform.localPosition = new Vector3(0, 2f, 0);
                    break;
                case "HideCone":
                    coneGO.SetActive(false);
                    break;
                case "HighlightMovement":
                    tutorialMvtCheck.gameObject.SetActive(true);
                    break;
                case "SpawnDummy":
                    SpawnDummy();
                    break;
                case "EnableSpells":
                    SpellCanvas.enabled = true;
                    PcRefs.gameObject.AddComponent<SpellCastController>().Init(PcRefs);
                    break;
                case "ZeroHealth":
                    break;
                case "RoundWin":
                    break;
                default:
                    break;
            }
        }
    }
}