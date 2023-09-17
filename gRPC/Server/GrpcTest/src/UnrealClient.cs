namespace Networking
{
    public class UnrealClient : GRPCClient
    {
        public int ID = default;
        
        public UnrealClient(int id)
        {
            ID = id;
        }
    }
}
