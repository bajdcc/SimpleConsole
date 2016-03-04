using System.IO;

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
