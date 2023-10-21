using Grpc.Core;
using GRPCServer;
using GRPCServer.Services;

namespace Networking
{
    public abstract class GRPCClient : IDisposable
    {
        public string Adress = "";
        public int id;

        public GRPCClient(string ad)
        {
            Adress = ad;
        }

        ~GRPCClient() 
        {
            Dispose();
        }


        public virtual void Disconnect()
        {
            /*Close all client related stream*/
            Dispose();
            MainServiceImpl.clients.Remove(Adress);
        }


        /// <summary>
        /// Dispose is already called in base.Disconnect()
        /// </summary>
        public abstract void Dispose();
    }
}
