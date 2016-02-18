using SimpleConsole.Typing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleConsole.Expression
{
    class LazyFun : Fun
    {
        private Func<string, IEnumerable<Expr>, Env, Result> evalLazy;

        public LazyFun(Func<string, IEnumerable<Expr>, Env, Result> func)
        {
            evalLazy = func;
            limit = false;
            writable = false;
        }

        public override string Name { get { return "Builtin"; } }

        public Result eval(string name, IEnumerable<Expr> exps, Env env)
        {
            return evalLazy(name, exps, env);
        }

        public override string ToString()
        {
            return $"lazy {name}{string.Concat(args.Select(a => " " + a))}";
        }
    }
}
