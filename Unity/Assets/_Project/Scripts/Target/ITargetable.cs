using Unity.Netcode;

namespace Project
{
    /// <summary>
    /// Use this with NetworkObject to allow targeting on network.
    /// </summary>
    public interface ITargetable
    {
        public void OnTargeted();
        public void OnUntargeted();
        
        /// <summary>
        /// </summary>
        /// <returns>NetworkID of associated object</returns>
        public NetworkObject GetNetworkObject();
    }
}