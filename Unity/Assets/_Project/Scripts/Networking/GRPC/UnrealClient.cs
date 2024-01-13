using System.Collections.Generic;
using Unity.Netcode;

namespace Project
{
    public class UnrealClient
    {
        // id Always negative 
        public int id;
        public string Address;
        public readonly string name;
        private readonly List<NetworkObject> _ownedNetObjects = new();

        private bool _isConnected;
        
        public UnrealClient(string ad, int id, string name)
        {
            Address = ad;
            _isConnected = true;
            this.id = id;
            this.name = name;
        }
        
        public void Disconnect()
        {
            if (!_isConnected) return;

            var copy = new List<NetworkObject>(_ownedNetObjects);
            foreach (var ownedNetObject in copy)
            {
                RemoveOwnership(ownedNetObject);
                
                ownedNetObject.Despawn();
            }
            
            _isConnected = false;
        }

        public void GiveOwnership(NetworkObject obj)
        {
            if (_ownedNetObjects.Contains(obj)) return;
            
            obj.GetSyncer().GiveUnrealOwnership(Address);
            _ownedNetObjects.Add(obj);
        }
        
        public void RemoveOwnership(NetworkObject obj)
        {
            if (!_ownedNetObjects.Contains(obj)) return;
            
            obj.GetSyncer().RemoveUnrealOwnership();
            _ownedNetObjects.Remove(obj);
        }
    }
}