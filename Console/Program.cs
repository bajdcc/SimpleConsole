﻿using SimpleConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("-----------------------");
            Console.WriteLine("Simple Console - bajdcc");
            Console.WriteLine("-----------------------");
            Console.WriteLine();
            var itpr = new Interpreter();
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
                    var val = itpr.input(input, out str);
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"{str} => {val}");
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