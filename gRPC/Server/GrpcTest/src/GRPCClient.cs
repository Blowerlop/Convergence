namespace Networking
{
    public abstract class GRPCClient
    {
        public int ID = default;

        public GRPCClient(int id)
        {
            ID = id;
        }

        public virtual void Disconnect() { /*Close all client related stream*/ }
    }
}
