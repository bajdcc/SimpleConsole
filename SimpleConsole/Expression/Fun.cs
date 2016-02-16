using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleConsole.Expression
{
    class Fun : Expr
    {
        public string name { set; get; }
        public List<string> args { set; get; }
        public Expr exp { set; get; }
        public override double eval(Env env)
        {
            return exp.eval(env);
        }
        public override string ToString()
        {
            return $"fn {name}{string.Concat(args.Select(a => " " + a))} => {exp}";
        }
    }
}
