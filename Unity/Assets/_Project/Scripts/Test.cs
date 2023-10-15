using System.Collections;
using System.Collections.Generic;
using BestHTTP.JSON;
using GRPCClient;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project
{
    public class Test : MonoBehaviour
    {
        [Button]
        private void TestMethod()
        {
            // GRPC_NetVarUpdate netVarUpdate = new GRPC_NetVarUpdate();;
            string encode = JsonConvert.SerializeObject(new Vector2(1, 3));
            Debug.Log(encode);


            Vector3 decode = JsonConvert.DeserializeObject<Vector3>(encode);
            Debug.Log(decode);
        }
    }
}
