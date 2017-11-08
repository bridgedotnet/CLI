using System;

namespace Bridge.CLI
{
    public partial class Program
    {
        private static void Error(string message, bool newLine = true)
        {
            WriteLine(message, newLine);
        }

        private static void Warn(string message, bool newLine = true)
        {
            WriteLine(message, newLine);
        }

        private static void Info(string message, bool newLine = true)
        {
            WriteLine(message, newLine);
        }

        private static void WriteLine(string message, bool newLine = true)
        {
            if (newLine)
            {
                Console.WriteLine(message);
            }
            else
            {
                Console.Write(message);
            }
        }
    }
}