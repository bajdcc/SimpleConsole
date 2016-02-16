using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleConsole.Expression
{
    abstract class Expr
    {
        public virtual double eval(Env env)
        {
            return 0.0;
        }
    }
}
