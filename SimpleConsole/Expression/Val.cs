using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleConsole.Expression
{
    class Val : Expr
    {
        public string name { set; get; }
        public double? val { set; get; }

        public override double eval(Env env)
        {
            return val ?? ((env.queryValue(name) as Val).val ?? 0.0);
        }
        public override string ToString()
        {
            return name == null ? (val ?? 0.0).ToString() : name;
        }
    }
}
