using GRPCServer.Services;
using Networking;
using System.Reactive.Linq;

namespace Utils
{
    public static class Streaming
    {
        public static async Task SafeStream(int clientId, Action streamAction)
        {
            try
            {
                await streamAction.ToAsync().Invoke();
            }
            catch (IOException)
            {
                MainServiceImpl.DisconnectClient(clientId);
            }
        }
    }
}
