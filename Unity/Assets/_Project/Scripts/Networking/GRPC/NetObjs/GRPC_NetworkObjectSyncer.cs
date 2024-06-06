using System;
using GRPCClient;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace Project
{
    
    [AddComponentMenu("GRPC/NetworkObject Syncer")]
    [RequireComponent(typeof(NetworkObject))]
    public class GRPC_NetworkObjectSyncer : NetworkBehaviour
    {
        [SerializeField, Required, InlineButton(nameof(GoToNotionDoc), "Notion")] 
        private string prefabId;

        [HideInInspector] public string UnrealOwnerAddress;
        [ShowInInspector] public bool IsOwnedByUnrealClient => !string.IsNullOrEmpty(UnrealOwnerAddress);

        public bool hasBeenSpawnedOnGrpc;
        public Action onNetworkObjectHasSpawnedOnGrpc;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            Debug.Log($"OnNetworkSpawn for NetworkObjectSyncer {prefabId} {gameObject.name}");
            
            if (!IsServer) return;
            
            Debug.Log($"OnNetworkSpawn for NetworkObjectSyncer IsServer true {prefabId} {gameObject.name}, IsConnected: {GRPC_NetworkManager.instance.isConnected}");
            
            if (GRPC_NetworkManager.instance.isConnected)
            {
                OnGrpcConnection_NetworkObjectSync();
            }
            GRPC_NetworkManager.instance.onClientStartedEvent += OnGrpcConnection_NetworkObjectSync;
        }
        
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            Debug.Log($"OnNetworkDespawn for NetworkObjectSyncer {prefabId} {gameObject.name}");
            
            if (!IsServer) return;
            
            Debug.Log($"OnNetworkDespawn for NetworkObjectSyncer IsServer true {prefabId} {gameObject.name}, IsConnected: {GRPC_NetworkManager.instance.isConnected}");
            
            if (GRPC_NetworkManager.instance.isConnected)
            {
                OnGrpDisconnection_NetworkObjectUnSync();
            }

            if (GRPC_NetworkManager.IsInstanceAlive())
            {
                GRPC_NetworkManager.instance.onClientStartedEvent -= OnGrpcConnection_NetworkObjectSync;
            }
        }
        
        private void OnGrpcConnection_NetworkObjectSync()
        {
            Debug.Log($"OnGrpcConnection_NetworkObjectSync {prefabId} {gameObject.name}");
            if (!EnsureInit()) return;
            
            Debug.Log($"OnGrpcConnection_NetworkObjectSync InitEnsured {prefabId} {gameObject.name}");
            
            GRPC_NetObjUpdate update = new()
            {
                NetId = (int)NetworkObject.NetworkObjectId, 
                Type = GRPC_NetObjUpdateType.New, 
                PrefabId = prefabId,
                Position = Utilities.UnityToGrpcVector3(transform.position),
                Rotation = Utilities.UnityToGrpcVector3(transform.rotation.eulerAngles)
            };

            GRPC_NetObjectsHandler.instance.SendNetObjsUpdate(update, this);
        }

        private void OnGrpDisconnection_NetworkObjectUnSync()
        {
            Debug.Log($"OnGrpDisconnection_NetworkObjectUnSync {prefabId} {gameObject.name}");
            if (!EnsureInit()) return;
            
            Debug.Log($"OnGrpDisconnection_NetworkObjectUnSync InitEnsured {prefabId} {gameObject.name}");
            
            GRPC_NetObjUpdate update = new()
            {
                NetId = (int)NetworkObject.NetworkObjectId, 
                Type = GRPC_NetObjUpdateType.Destroy
            };
            
            GRPC_NetObjectsHandler.instance.SendNetObjsUpdate(update, this);

            if (IsOwnedByUnrealClient)
            {
                GRPC_NetworkManager.instance.GetUnrealClientByAddress(UnrealOwnerAddress).RemoveOwnership(NetworkObject);
            }
        }

        private bool EnsureInit()
        {
            if (!GRPC_NetworkManager.instance.isConnected)
            {
                Debug.LogError($"Trying to sync NetworkObject {prefabId} without being connected to the GRPC server.");
                return false;
            }

            if (GRPC_NetObjectsHandler.IsInstanceAlive() == false)
            {
                Debug.LogError($"Trying to sync NetworkObject {prefabId} without an instance of GRPC_NetObjectsHandler.");
                return false;
            }

            return true;
        }
        
        private void GoToNotionDoc()
        {
            Application.OpenURL("https://www.notion.so/Prefabs-Variables-47c9360e1da14c58999e720174cf88aa?pvs=4#771b560f3e314680b12450a828a299cc");
        }
        
#if UNITY_EDITOR
        [MenuItem("GameObject/GRPC/Synced NetworkObject", false, 1)]
        private static void CreateObject(MenuCommand menuCommand)
        {
            var parent = menuCommand.context as GameObject;
            
            var instance = ObjectFactory.CreateGameObject("GRPC Synced NetworkObject", typeof(GRPC_NetworkObjectSyncer));

            if (parent != null) instance.transform.SetParent(parent.transform);
            
            GameObjectUtility.EnsureUniqueNameForSibling(instance);
            
            // This call ensure any change made to created Objects after they where registered will be part of the Undo.
            Undo.RegisterFullObjectHierarchyUndo(parent == null ? instance : parent, "");

            // We have to fix up the undo name since the name of the object was only known after reparenting it.
            Undo.SetCurrentGroupName("Create " + instance.name);

            Selection.activeGameObject = instance;
        }
#endif

        public void GiveUnrealOwnership(string address)
        {
            UnrealOwnerAddress = address;
        }
        
        public void RemoveUnrealOwnership()
        {
            UnrealOwnerAddress = null;
        }
    }
}
