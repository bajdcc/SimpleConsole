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
        public virtual Result eval(Env env)
        {
            return Result.Empty;
        }
    }
}
