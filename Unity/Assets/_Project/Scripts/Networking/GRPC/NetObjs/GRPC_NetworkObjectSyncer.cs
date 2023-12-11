using System;
using System.Collections.Generic;
using System.Reflection;
using GRPCClient;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Project
{
    [AddComponentMenu("GRPC/NetworkObject Syncer")]
    [RequireComponent(typeof(NetworkObject))]
    public class GRPC_NetworkObjectSyncer : NetworkBehaviour
    {
        [SerializeField, Required, InlineButton(nameof(GoToNotionDoc), "Notion")] 
        private string prefabId;

        [HideInInspector] public string UnrealOwnerAddress;
        public bool IsOwnedByUnrealClient => !string.IsNullOrEmpty(UnrealOwnerAddress);

        
        private void Start()
        {
            if (!IsServer && !IsHost) return;

            
            if (GRPC_NetworkManager.instance.isConnected)
            {
                OnGrpcConnection_NetworkObjectSync();
            }
            GRPC_NetworkManager.instance.onClientStartedEvent.Subscribe(this, OnGrpcConnection_NetworkObjectSync);

            if (IsOwnedByUnrealClient)
            {
                GRPC_NetworkManager.instance.onUnrealClientDisconnect.Subscribe(this, _ => OnGrpDisconnection_NetworkObjectUnSync());
            }
            else
            {
                GRPC_NetworkManager.instance.onClientStopEvent.Subscribe(this, OnGrpDisconnection_NetworkObjectUnSync);
            }
        }

        // public override void OnDestroy()
        // {
        //     base.OnDestroy();
        //
        //     if (GRPC_NetworkManager.isBeingDestroyed == false)
        //     {
        //         GRPC_NetworkManager.instance.onClientStartedEvent.Unsubscribe(OnGrpcConnection_NetworkObjectSync);
        //         if (IsOwnedByUnrealClient)
        //         {
        //             GRPC_NetworkManager.instance.onUnrealClientDisconnect.Unsubscribe(_ => OnGrpDisconnection_NetworkObjectUnSync());
        //         }
        //         else
        //         {
        //             GRPC_NetworkManager.instance.onClientStartedEvent.Unsubscribe(OnGrpDisconnection_NetworkObjectUnSync);
        //         }
        //     }
        // }

        private void OnGrpcConnection_NetworkObjectSync()
        {
            if (!EnsureInit()) return;
            
            GRPC_NetObjUpdate update = new()
            {
                NetId = (int)NetworkObject.NetworkObjectId, 
                Type = GRPC_NetObjUpdateType.New, 
                PrefabId = prefabId
            };

            GRPC_NetObjectsHandler.instance.SendNetObjsUpdate(update);
        }

        private void OnGrpDisconnection_NetworkObjectUnSync()
        {
            if (!EnsureInit()) return;
            
            GRPC_NetObjUpdate update = new()
            {
                NetId = (int)NetworkObject.NetworkObjectId, 
                Type = GRPC_NetObjUpdateType.Destroy
            };
            
            GRPC_NetObjectsHandler.instance.SendNetObjsUpdate(update);

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

            if (!GRPC_NetObjectsHandler.instance)
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
