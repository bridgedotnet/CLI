using System;
using System.Threading;

namespace Bridge.CLI
{
    public class ConsoleSpinner : IDisposable
    {
        private const string Sequence = @"/-\|";
        private int counter = 0;
        private readonly int left;
        private readonly int top;
        private readonly int delay;
        private bool active;
        private readonly Thread thread;

        public ConsoleSpinner(int delay = 100) : this(Console.CursorLeft, Console.CursorTop, delay)
        {
        }

        public ConsoleSpinner(int left, int top, int delay = 100)
        {
            this.left = left;
            this.top = top;
            this.delay = delay;
            thread = new Thread(Spin);
        }

        public void Start()
        {
            active = true;
            if (!thread.IsAlive)
                thread.Start();
        }

        public void Stop()
        {
            if (active)
            {
                active = false;
                Clear();
            }
        }

        private void Spin()
        {
            while (active)
            {
                Turn();
                Thread.Sleep(delay);
            }

            Draw(' ');
        }

        private void Draw(char c)
        {
            if (active)
            {
                ForceDraw(c);
            }
        }

        private void Clear()
        {
            Console.SetCursorPosition(left, top);
            Console.Write("");
        }

        private void ForceDraw(char c)
        {
            Console.SetCursorPosition(left, top);
            Console.Write(c);
        }

        private void Turn()
        {
            Draw(Sequence[++counter % Sequence.Length]);
        }

        public void Dispose()
        {
            Stop();
        }
    }
}