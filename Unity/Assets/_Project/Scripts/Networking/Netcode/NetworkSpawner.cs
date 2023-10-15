using System;
using System.Numerics;
using GRPCClient;
using Unity.Netcode;
using UnityEngine;

namespace Project
{
    public class NetworkSpawner : MonoBehaviour
    {
        public static T Spawn<T>(T prefab, Action<T> setter = null) where T : Component
        {
            T obj = Instantiate(prefab);
            setter?.Invoke(obj);

            var netObj = obj.GetComponent<NetworkObject>();

            if (netObj == null)
                throw new NullReferenceException($"Prefab {prefab.name} does not have a NetworkObject component.");

            netObj.Spawn();

            if (GRPC_NetObjectsHandler.instance)
            {
                GRPC_NetObjUpdate update = new()
                {
                    NetId = (int)netObj.NetworkObjectId, 
                    Type = GRPC_NetObjUpdateType.New, 
                    PrefabId = prefab.name
                };
                GRPC_NetObjectsHandler.instance.SendNetObjsUpdate(update);
            }
            
            return obj;
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
    }
}