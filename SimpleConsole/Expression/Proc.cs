using System;
using SimpleConsole.Typing;
using System.Collections.Generic;
using System.Linq;

namespace SimpleConsole.Expression
{
    internal class Proc : Expr
    {
        /// <summary>
        /// 调用原函数
        /// </summary>
        public Fun Fun { set; get; }

        /// <summary>
        /// 实参
        /// </summary>
        public IEnumerable<Expr> Args { set; get; }

        public override Result Eval(Env env)
        {
            var lazy = Fun as LazyFun;
            if (lazy != null)
            {
                if (Args.Count() == 1)
                    throw new ScException("至少需要一个参数");
                var name = (Args.First() as Val)?.ValName;
                if (name == null) throw new ArgumentNullException(nameof(name));
                return lazy.Eval(name, Args.Skip(1).ToList(), env);
            }
            var evalArgs = (Fun is BuiltinFun ? Args.Skip(1) : Args).Select(a => a.Eval(env)).ToList();
            var count = Fun.Args.Count();
            env.PushNewEnv();
            if (Fun.Limit)
            {
                if (Fun is BuiltinFun)
                {
                    env.PutValue(Fun.Args.First(), Args.First());
                    if (Fun.Args.Count() > 1)
                        env.PutValue(Fun.Args.Skip(1).First(), new Val() { Result = new Result(evalArgs) });
                }
                else
                {
                    for (var i = 0; i < count; i++)
                    {
                        env.PutValue(Fun.Args.ElementAt(i), new Val() { Result = evalArgs.ElementAt(i) });
                    }
                }                
            }
            else
            {
                switch (count)
                {
                    case 0:
                        throw new ScException("至少要有一个实参");
                    case 1:
                        env.PutValue(Fun.Args.First(), new Val() { Result = new Result(evalArgs) });
                        break;
                    default:
                        var arg = new Result(evalArgs);
                        env.PutValue(Fun.Args.First(), Fun is BuiltinFun ? Args.First() : new Val()
                        {
                            Result =
                                new Result() { Type = arg.Type, Val = arg.Val.Take(1) }
                        });
                        env.PutValue(Fun.Args.Skip(1).First(), new Val()
                        {
                            Result =
                                new Result() { Type = arg.Type, Val = Fun is BuiltinFun ? arg.Val : arg.Val.Skip(1) }
                        });
                        break;
                }
            }
            var v = Fun.Eval(env);
            env.PopEnv();
            return v;
        }

        public override string ToString()
        {
            return $"{Fun.FunName}{string.Concat(Args.Select(a => " " + a.ToString()))}";
        }
    }
}
