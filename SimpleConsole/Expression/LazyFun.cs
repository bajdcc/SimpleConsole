using SimpleConsole.Typing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleConsole.Expression
{
    internal class LazyFun : Fun
    {
        private readonly Func<string, IList<Expr>, Env, Result> _evalLazy;

        public LazyFun(Func<string, IList<Expr>, Env, Result> func)
        {
            _evalLazy = func;
            Limit = false;
            Writable = false;
        }

        public override string Name => "Lazy";

        public Result Eval(string funName, IList<Expr> exps, Env env)
        {
            if (funName == null) throw new ArgumentNullException(nameof(funName));
            return _evalLazy(funName, exps, env);
        }

        public override string ToString()
        {
            return $"lazy {FunName}{string.Concat(Args.Select(a => " " + a))}";
        }
    }
}
