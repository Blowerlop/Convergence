using System.Diagnostics;

namespace Utils
{
    public static class MTime
    {
        //Everything in ms

        const int fps = 20;
        const int timePerFrame = 1000 / fps; //in ms

        static int deltaTime;

        static int timer = 0;

        public static event Action onTickCall;

        public static async Task CountTickRate()
        {
            Stopwatch watch = new Stopwatch();

            while (true)
            {
                deltaTime = (int)watch.ElapsedMilliseconds;
                watch.Restart();

                timer += deltaTime;

                while (timer >= timePerFrame)
                {
                    timer -= timePerFrame;

                    onTickCall?.Invoke();
                }

                while ((int)watch.ElapsedMilliseconds < timePerFrame) { }
            }
        }
    }
}