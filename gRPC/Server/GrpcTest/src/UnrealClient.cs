namespace Networking
{
    public class UnrealClient : GRPCClient
    {
        public UnrealClient(int id) : base(id) { }

        public override void Disconnect()
        {
            base.Disconnect();
        }
    }
}
