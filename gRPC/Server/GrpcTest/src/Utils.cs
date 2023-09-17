using System.Reactive.Linq;

namespace Utils
{
    public class Streaming
    {
        public static async Task SafeStream(Action streamAction)
        {
            try
            {
                await streamAction.ToAsync().Invoke();
            }
            catch (IOException)
            {
                //Disconnect client
            }
        }
    }
}
