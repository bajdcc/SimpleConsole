using SimpleConsole;
using System;

namespace TestConsole
{
    static class Program
    {
        static void Main(string[] args)
        {
            var itpr = InterpreterFactory.Create(Console.In, Console.Out);
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("> ");                
                var input = Console.ReadLine();
                if (input == "exit")
                    break;
                try
                {
                    string str;
                    var val = itpr.Input(input, out str);
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    itpr.Print(val);
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"错误：{e.Message}");
                }
                finally
                {
                    Console.ResetColor();
                }
            }
        }
    }
}
