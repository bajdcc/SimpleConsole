using SimpleConsole.Typing;
using System.Collections.Generic;
using System.Linq;

namespace SimpleConsole.Expression
{
    internal class Fun : Expr
    {
        /// <summary>
        /// 是否为有限参数
        /// </summary>
        public bool Limit { set; get; } = true;
        /// <summary>
        /// 函数名
        /// </summary>
        public string FunName { set; get; }
        /// <summary>
        /// 形参
        /// </summary>
        public IEnumerable<string> Args { set; get; }
        /// <summary>
        /// 表达式
        /// </summary>
        public Expr Exp { set; get; }

        /// <summary>
        /// 可修改
        /// </summary>
        public bool Writable { set; get; } = true;

        public override string Name => "Function";

        public override Result Eval(Env env)
        {
            if (env.IsTopEnv())
            {
                if (!(Limit && !Args.Any()))
                    return Result.Empty;
            }
            return Exp.Eval(env);
        }

        public void RegisterToEnv(Env env)
        {
            env.PutValue(FunName, this);
        }

        public override string ToString()
        {
            return (Limit ? "fn" : "fnx") + $" {FunName}{string.Concat(Args.Select(a => " " + a))} => {Exp}";
        }
    }
}
