using SimpleConsole.Typing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleConsole.Expression
{
    class Fun : Expr
    {
        /// <summary>
        /// 是否为有限参数
        /// </summary>
        public bool limit { set; get; } = true;
        /// <summary>
        /// 函数名
        /// </summary>
        public string name { set; get; }
        /// <summary>
        /// 形参
        /// </summary>
        public IEnumerable<string> args { set; get; }
        /// <summary>
        /// 表达式
        /// </summary>
        public Expr exp { set; get; }

        /// <summary>
        /// 可修改
        /// </summary>
        public bool writable { set; get; } = true;

        public override string Name { get { return "Function"; } }

        public override Result eval(Env env)
        {
            if (env.isTopEnv())
            {
                if (!(limit && args.Count() == 0))
                    return Result.Empty;
            }
            return exp.eval(env);
        }

        public void RegisterToEnv(Env env)
        {
            env.putValue(name, this);
        }

        public override string ToString()
        {
            return (limit ? "fn" : "fnx") + $" {name}{string.Concat(args.Select(a => " " + a))} => {exp}";
        }
    }
}
