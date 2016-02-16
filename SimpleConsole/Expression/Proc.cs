using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleConsole.Expression
{
    class Proc : Expr
    {
        public Fun fun { set; get; }
        public List<Expr> args { set; get; }

        public override double eval(Env env)
        {
            var evalArgs = args.Select(a => a.eval(env)).ToList();
            env.pushNewEnv();
            for (int i = 0; i < args.Count; i++)
            {
                env.putValue(fun.args[i], new Val() { val = evalArgs[i] });
            }
            var v = fun.eval(env);
            env.popEnv();
            return v;
        }
        public override string ToString()
        {
            return $"{fun.name}{string.Concat(args.Select(a => " " + a.ToString()))}";
        }
    }
}
