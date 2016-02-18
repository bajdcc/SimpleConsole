using SimpleConsole.Typing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleConsole.Expression
{
    class BuiltinFun : Fun
    {
        private Func<string, Result, Result> evalBuiltin;

        public BuiltinFun(Func<string, Result, Result> func)
        {
            evalBuiltin = func;
            limit = false;
            writable = false;
        }

        public override string Name { get { return "Builtin"; } }

        public override Result eval(Env env)
        {
            var x = env.queryValue(args.First()) as Val;
            if (args.Count() == 1)
                return evalBuiltin(x.name, Result.Empty);
            var xs = env.queryValue(args.Skip(1).First()) as Val;
            return evalBuiltin(x.name, xs.result);
        }

        public override string ToString()
        {
            return $"builtin {name}{string.Concat(args.Select(a => " " + a))}";
        }
    }
}
