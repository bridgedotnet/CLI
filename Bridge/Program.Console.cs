using System;

namespace Bridge.CLI
{
    public partial class Program
    {
        private static void Error(string message, bool newLine = true)
        {
            WriteLine(message, ConsoleColor.Red, newLine);
        }

        private static void Warn(string message, bool newLine = true)
        {
            WriteLine(message, ConsoleColor.Yellow, newLine);
        }

        private static void Info(string message, bool newLine = true)
        {
            WriteLine(message, ConsoleColor.Green, newLine);
        }

        private static void WriteLine(string message, ConsoleColor color, bool newLine = true)
        {
            Console.ForegroundColor = color;

            if (newLine)
            {
                Console.WriteLine(message);
            }
            else
            {
                Console.Write(message);
            }

            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}