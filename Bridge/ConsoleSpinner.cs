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
        private static bool? dumbTerm = null;

        /// <summary>
        /// This will ensure the console can handle "fancy" operations with
        /// the cursor. Dumb terminals or cygwin shell won't handle it.
        /// </summary>
        public static bool DumbTerm
        {
            get
            {
                if (dumbTerm == null)
                {
                    try
                    {
                        Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop);
                        dumbTerm = false;
                    }
                    catch (System.IO.IOException)
                    {
                        dumbTerm = true;
                    }
                    catch
                    {
                        throw;
                    }
                }

                return dumbTerm.Value;
            }
        }

        public ConsoleSpinner(int delay = 100) : this(-1, -1, delay)
        {
        }

        public ConsoleSpinner(int left, int top, int delay = 100)
        {
            this.delay = delay;

            if (DumbTerm)
            {
                this.left = -1;
                this.top = -1;
                thread = new Thread(DropDots);
            }
            else
            {
                this.left = left < 0 ? Console.CursorLeft : left;
                this.top = top < 0 ? Console.CursorTop : top;
                thread = new Thread(Spin);
            }
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

        private void DropDots()
        {
            while (active)
            {
                Console.Write('.');
                Thread.Sleep(delay);
            }
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
            if (!DumbTerm)
            {
                Console.SetCursorPosition(left, top);
                Console.Write("");
            }
            else
            {
                // On dumb terminals, "clear" will mean printing a whitespace after the last printed dot.
                Console.Write(" ");
            }
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