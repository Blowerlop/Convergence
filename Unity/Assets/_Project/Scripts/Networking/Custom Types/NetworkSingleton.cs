// using Unity.Netcode;
// using UnityEngine;
//
// namespace Project
// {
//     public class NetworkSingleton<T> : NetworkBehaviour where T : NetworkBehaviour
//     {
//         protected static bool dontDestroyOnLoad = true;
//         protected static bool isBeingDestroyed = false;
//         
//         private static T _instance = null;
//         public static T instance {
//             get { 
//                 if(_instance == null){
//                     _instance = FindObjectOfType<T>();
//                     if(_instance == null){
//                         GameObject singletonObj = new GameObject();
//                         singletonObj.name = typeof(T).ToString();
//                         _instance = singletonObj.AddComponent<T>();
//                         
//                         NetworkObject networkObject = singletonObj.AddComponent<NetworkObject>();
//                         if (networkObject == null)
//                         {
//                             isBeingDestroyed = true;
//                             Destroy(singletonObj);
//                             return null;
//                         }
//                         
//                         networkObject.SpawnWithOwnership(NetworkManager.Singleton.LocalClientId, dontDestroyOnLoad);
//                     }
//                 }
//                 return _instance;
//             }
//         }
//         
//         protected virtual void Awake(){
//             if (_instance != null)
//             {
//                 Debug.LogError($"There is more than one instance of {this}");
//                 isBeingDestroyed = true;
//                 Destroy(this);
//                 return;
//             }
//     
//             _instance = GetComponent<T>();
//         }
//         
//         public override void OnDestroy()
//         {
//             base.OnDestroy();
//             isBeingDestroyed = false;
//         }
//     
//         public static bool IsInstanceAlive() => _instance != null;
//     
//         [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
//         private static void Reset()
//         {
//             _instance = null;
//         }
//     }
// }
