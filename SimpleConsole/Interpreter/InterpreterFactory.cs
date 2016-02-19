using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleConsole
{
    public static class InterpreterFactory
    {
        public static IInterpreter Create(TextReader tr, TextWriter tw)
        {
            return new Interpreter(tr, tw);
        }
    }
}
