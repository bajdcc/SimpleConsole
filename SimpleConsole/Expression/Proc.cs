using SimpleConsole.Typing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleConsole.Expression
{
    class Proc : Expr
    {
        /// <summary>
        /// 调用原函数
        /// </summary>
        public Fun fun { set; get; }

        /// <summary>
        /// 实参
        /// </summary>
        public List<Expr> args { set; get; }

        public override Result eval(Env env)
        {
            if (fun is BuiltinFun)
            {
                var evalArgs = args.Skip(1).Take(args.Count - 1).Select(a => a.eval(env)).ToList();
                env.pushNewEnv();
                if (args.Count == 0)
                    throw new SCException("至少要有一个实参");
                env.putValue(fun.args[0], args[0] as Val);
                env.putValue(fun.args[1], new Val() { result = new Result(evalArgs) });
                var v = fun.eval(env);
                env.popEnv();
                return v;
            }
            else
            {
                var evalArgs = args.Select(a => a.eval(env)).ToList();
                env.pushNewEnv();
                if (fun.limit)
                {
                    for (int i = 0; i < args.Count; i++)
                    {
                        env.putValue(fun.args[i], new Val() { result = evalArgs[i] });
                    }
                }
                else
                {
                    if (args.Count == 0)
                        throw new SCException("至少要有一个实参");
                    env.putValue(fun.args[0], new Val() { result = evalArgs[0] });
                    evalArgs.RemoveAt(0);
                    env.putValue(fun.args[1], new Val() { result = new Result(evalArgs) });
                }
                var v = fun.eval(env);
                env.popEnv();
                return v;
            }
        }

        public override string ToString()
        {
            return $"{fun.name}{string.Concat(args.Select(a => " " + a.ToString()))}";
        }
    }
}
