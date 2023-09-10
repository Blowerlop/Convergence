using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Utils;

namespace TestServer.Services
{
    public class TestServiceImpl : TestService.TestServiceBase
    {
        private readonly ILogger<TestServiceImpl> _logger;
        public TestServiceImpl(ILogger<TestServiceImpl> logger)
        {
            _logger = logger;
            MTime.onTickCall += SendPositions;
        }

        #region Hello
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            Console.WriteLine(request.Msg);

            return Task.FromResult(new HelloReply
            {
                Msg = "Hello back !"
            });
        }
        #endregion

        #region Health
        int[] playersHealth = new int[4];
        private static List<IServerStreamWriter<HealthGet>> clientHealthStreams = new();

        public override async Task SubscribeToPlayersHealth(EmptyMsg request, IServerStreamWriter<HealthGet> responseStream, ServerCallContext context)
        {
            Console.WriteLine(context.Host + " subscribes.");

            clientHealthStreams.Add(responseStream);

            while (!context.CancellationToken.IsCancellationRequested)
            {
                await Task.Delay(2500);
            }
        }

        public override Task<EmptyMsg> SendPlayerHealth(HealthPost request, ServerCallContext context)
        {
            Console.WriteLine("Received health => playerIndex: " + request.PlayerIndex + ", health: " + request.Health);

            HealthGet reply = new HealthGet() 
            { 
                PlayerIndex = request.PlayerIndex, 
                Health = request.Health 
            };

            playersHealth[reply.PlayerIndex] = reply.Health;

            clientHealthStreams.ForEach(async x => await x.WriteAsync(reply));

            return Task.FromResult(new EmptyMsg());
        }
        #endregion

        #region Positions
        static List<Vector3> playersPosition = new ();

        static List<IServerStreamWriter<PositionGet>> clientPositionStreams = new();

        public override async Task PlayerPosition(IAsyncStreamReader<PositionPost> requestStream, IServerStreamWriter<PositionGet> responseStream, ServerCallContext context)
        {
            Console.WriteLine(context.Host + " subscribes.");

            clientPositionStreams.Add(responseStream);
            playersPosition.Add(new Vector3());

            await foreach (var msg in requestStream.ReadAllAsync())
            {
                playersPosition[msg.PlayerIndex] = new Vector3() { X = msg.Position.X, Y = msg.Position.Y, Z = msg.Position.Z };
            }
        }

        async void SendPositions()
        {
            if (clientPositionStreams.Count <= 0) return;

            //Console.WriteLine("Send positions: " + clientPositionStreams.Count);

            PositionGet msg = new PositionGet();
            msg.Positions.AddRange(playersPosition);

            foreach(var stream in clientPositionStreams)
            {
                await stream.WriteAsync(msg);
            }
        } 
        #endregion
    }
}