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
            if (fun is LazyFun)
            {
                if (args.Count() == 1)
                    throw new SCException("至少需要一个参数");
                var lazy = fun as LazyFun;
                var name = (args.First() as Val).name;
                return lazy.eval(name, args.Skip(1), env);
            }
            var evalArgs = (fun is BuiltinFun ? args.Skip(1) : args).Select(a => a.eval(env)).ToList();
            var count = fun.args.Count();
            env.pushNewEnv();
            if (fun.limit)
            {
                if (fun is BuiltinFun)
                {
                    env.putValue(fun.args.First(), args.First());
                    if (fun.args.Count() > 1)
                        env.putValue(fun.args.Skip(1).First(), new Val() { result = new Result(evalArgs) });
                }
                else
                {
                    for (int i = 0; i < count; i++)
                    {
                        env.putValue(fun.args.ElementAt(i), new Val() { result = evalArgs.ElementAt(i) });
                    }
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
                    var arg = new Result(evalArgs);
                    env.putValue(fun.args.First(), fun is BuiltinFun ? args.First() : new Val()
                    {
                        result =
                        new Result() { type = arg.type, val = arg.val.Take(1) }
                    });
                    env.putValue(fun.args.Skip(1).First(), new Val()
                    {
                        result =
                        new Result() { type = arg.type, val = fun is BuiltinFun ? arg.val : arg.val.Skip(1) }
                    });
                }
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
