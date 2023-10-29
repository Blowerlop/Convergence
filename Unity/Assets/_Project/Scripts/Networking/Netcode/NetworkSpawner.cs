using System;
using System.Linq;
using GRPCClient;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Project
{
    //Is MonoBehaviour because it needs to Instantiate
    public class NetworkSpawner : MonoBehaviour
    {
        public static T Spawn<T>(T prefab, Vector3 position, bool destroyWithScene = false) where T : Component
        {
            return Spawn(prefab, (obj) => obj.transform.position = position, destroyWithScene);
        }
        
        public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation, bool destroyWithScene = false) where T : Component
        {
            return Spawn(prefab, (obj) =>
            {
                var t = obj.transform;
                t.position = position;
                t.rotation = rotation;
            }, destroyWithScene);
        }
        
        public static T Spawn<T>(T prefab, Action<T> setter = null, bool destroyWithScene = false) where T : Component
        {
            var instance = InstantiateBaseObject(prefab, setter);

            instance.netObj.Spawn(destroyWithScene);

            SendGrpcSpawnUpdate(instance.netObj, prefab.name);
            
            return instance.baseObj;
        }

        public static T SpawnWithOwnership<T>(T prefab, ulong clientId, Action<T> setter = null, bool destroyWithScene = false) where T : Component
        {
            var instance = InstantiateBaseObject(prefab, setter);

            instance.netObj.SpawnWithOwnership(clientId, destroyWithScene);

            SendGrpcSpawnUpdate(instance.netObj, prefab.name);
            
            return instance.baseObj;
        }
        
        public static T SpawnAsPlayerObject<T>(T prefab, ulong clientId, Action<T> setter = null, bool destroyWithScene = false) where T : Component
        {
            var instance = InstantiateBaseObject(prefab, setter);

            instance.netObj.SpawnAsPlayerObject(clientId, destroyWithScene);

            SendGrpcSpawnUpdate(instance.netObj, prefab.name);
            
            return instance.baseObj;
        }
        
        private static (T baseObj, NetworkObject netObj) InstantiateBaseObject<T>(T prefab, Action<T> setter = null) where T : Component
        {
            T obj = Instantiate(prefab);
            setter?.Invoke(obj);
            
            var netObj = obj.GetComponent<NetworkObject>();

            if (netObj == null)
                throw new NullReferenceException($"Prefab {prefab.name} does not have a NetworkObject component.");

            return (baseObj: obj, netObj: netObj);
        }
        
        private static void SendGrpcSpawnUpdate(NetworkObject netObj, string prefabId)
        {
            if(!GRPC_NetworkManager.instance.isConnected) return;
            
            if (GRPC_NetObjectsHandler.instance)
            {
                GRPC_NetObjUpdate update = new()
                {
                    NetId = (int)netObj.NetworkObjectId, 
                    Type = GRPC_NetObjUpdateType.New, 
                    PrefabId = prefabId
                };
                GRPC_NetObjectsHandler.instance.SendNetObjsUpdate(update);
            }
        }
        
        public static void Despawn(NetworkObject netObj)
        {
            if (GRPC_NetObjectsHandler.instance)
            {
                GRPC_NetObjUpdate update = new() 
                { 
                    NetId = (int)netObj.NetworkObjectId, 
                    Type = GRPC_NetObjUpdateType.Destroy
                };
                
                GRPC_NetObjectsHandler.instance.SendNetObjsUpdate(update);
            }

            netObj.Despawn();
        }
        
        #region Debug
        
        [ConsoleCommand("dbg_spawn", "Spawn a dummy network object to test sync between Unreal and Unity.")]
        public static void TestSpawnCmd(string name)
        {
            GameObject prefab = null;
            
            switch (name)
            {
                case "Player":
                    prefab = GRPC_NetObjectsHandler.instance.playerPrefab;
                    break;
                case "Cube":
                    prefab = GRPC_NetObjectsHandler.instance.cubePrefab;
                    break;
                default:
                    prefab = GRPC_NetObjectsHandler.instance.cubePrefab;
                    break;
            }
            
            Spawn(prefab.transform, x =>
            {
                x.position = Vector3.right * Random.Range(-5f, 5f);
            });
        }

        [ConsoleCommand("dbg_spawn_player", "Spawn the player network object")]
        public static void TestSpawnPlayer(int clientIndex)
        {
            SpawnAsPlayerObject(GRPC_NetObjectsHandler.instance.playerPrefab.transform,
                NetworkManager.Singleton.ConnectedClientsIds[clientIndex],
                x =>
                {
                    x.position = Vector3.forward * Random.Range(-2f, 2f);
                });
        }
        
        
        [ConsoleCommand("dbg_spawn_ownership", 
            "Spawn a dummy network object with ownership to test sync between Unreal and Unity.")]
        public static void TestSpawnWithOwnershipCmd(int clientIndex = 0)
        {
            SpawnWithOwnership(GRPC_NetObjectsHandler.instance.playerPrefab.transform,
                NetworkManager.Singleton.ConnectedClientsIds[clientIndex],
                x =>
                {
                    x.position = Vector3.up * Random.Range(-5f, 5f);
                });
        }
        
        [ConsoleCommand("dbg_spawn_player_object", 
            "Spawn a dummy network object as player object to test sync between Unreal and Unity.")]
        public static void TestSpawnAsPlayerObjectCmd(int clientIndex = 0)
        {
            SpawnAsPlayerObject(GRPC_NetObjectsHandler.instance.playerPrefab.transform,
                NetworkManager.Singleton.ConnectedClientsIds[clientIndex],
                x =>
                {
                    x.position = Vector3.forward * Random.Range(-2f, 2f);
                });
        }
        
        [ConsoleCommand("dbg_despawn", 
            "Despawn random network object to test sync between Unreal and Unity.")]
        public static void TestDespawnCmd()
        {
            var spawnedObjects = NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values.ToList();
                
            var rand = spawnedObjects[Random.Range(0, spawnedObjects.Count)];
                
            Despawn(rand);
        }
        
        #endregion
    }
}