namespace Networking
{
    public class UnrealClient : GRPCClient
    {
        public UnrealClient(string ad) : base(ad) { }

        public override void Disconnect()
        {
            base.Disconnect();
        }
    }
}
