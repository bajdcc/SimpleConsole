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
        public IEnumerable<Expr> args { set; get; }

        public override Result eval(Env env)
        {
            var count = fun.args.Count();
            if (fun is BuiltinFun)
            {
                var evalArgs = args.Skip(1).Select(a => a.eval(env)).ToList();
                env.pushNewEnv();
                if (count == 0)
                    throw new SCException("至少要有一个实参");
                env.putValue(fun.args.First(), args.First() as Val);
                env.putValue(fun.args.Skip(1).First(), new Val() { result = new Result(evalArgs) });
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
                    for (int i = 0; i < count; i++)
                    {
                        env.putValue(fun.args.ElementAt(i), new Val() { result = evalArgs.ElementAt(i) });
                    }
                }
                else
                {
                    if (count == 0)
                        throw new SCException("至少要有一个实参");
                    if (count == 1)
                    {
                        env.putValue(fun.args.First(), new Val() { result = new Result(evalArgs) });
                    }
                    else
                    {
                        env.putValue(fun.args.First(), new Val() { result = evalArgs.First() });
                        env.putValue(fun.args.Skip(1).First(), new Val() { result = new Result(evalArgs.Skip(1)) });
                    }
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
