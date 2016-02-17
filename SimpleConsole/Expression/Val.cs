using SimpleConsole.Typing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleConsole.Expression
{
    class Val : Expr
    {
        /// <summary>
        /// 变量名
        /// </summary>
        public string name { set; get; }

        /// <summary>
        /// 结果
        /// </summary>
        public Result result { set; get; } = Result.Empty;

        /// <summary>
        /// 可修改
        /// </summary>
        public bool writable { set; get; } = true;

        public override Result eval(Env env)
        {
            return result.IsEmpty ? env.queryValue(name).eval(env) : result;
        }

        public override string ToString()
        {
            return name ?? result.ToString();
        }
    }        
}
