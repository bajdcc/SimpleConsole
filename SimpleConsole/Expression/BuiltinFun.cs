using SimpleConsole.Typing;
using System;
using System.Linq;

namespace SimpleConsole.Expression
{
    internal class BuiltinFun : Fun
    {
        private readonly Func<string, Result, Result> _evalBuiltin;

        public BuiltinFun(Func<string, Result, Result> func)
        {
            _evalBuiltin = func;
            Limit = false;
            Writable = false;
        }

        public override string Name => "Builtin";

        public override Result Eval(Env env)
        {
            var x = env.QueryValue(Args.First()) as Val;
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (Args.Count() == 1)
                return _evalBuiltin(x.ValName, Result.Empty);
            var xs = env.QueryValue(Args.Skip(1).First()) as Val;
            if (xs == null) throw new ArgumentNullException(nameof(xs));
            return _evalBuiltin(x.ValName, xs.Result);
        }

        public override string ToString()
        {
            return $"builtin {FunName}{string.Concat(Args.Select(a => " " + a))}";
        }
    }
}
