namespace Networking
{
    public abstract class GRPCClient
    {
        public string Adress = "";

        public GRPCClient(string ad)
        {
            Adress = ad;
        }

        public virtual void Disconnect() { /*Close all client related stream*/ }
    }
}
