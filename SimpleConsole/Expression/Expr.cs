using SimpleConsole.Typing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleConsole.Expression
{
    abstract class Expr
    {
        public virtual string Name { get; }

        public virtual Result eval(Env env)
        {
            return Result.Empty;
        }

        /// <summary>
        /// 返回最左原子子表达式
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual Expr GetMostLeftCombineAtom(OpType type, Expr parent)
        {
            return null;
        }
    }
}
