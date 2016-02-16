using SimpleConsole.Expression;
using SimpleConsole.Typing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleConsole
{
    class Builtin
    {
        private Dictionary<string, Func<Result, Result>> mapBuiltins = new Dictionary<string, Func<Result, Result>>();

        public Builtin()
        {
            init();
        }

        private void init()
        {
            initMath();
        }

        private void initMath()
        {
            mapBuiltins.Add("sin", a => a.convert(b => Math.Sin(Convert.ToDouble(b)), ResultType.Double));
            mapBuiltins.Add("cos", a => a.convert(b => Math.Cos(Convert.ToDouble(b)), ResultType.Double));
            mapBuiltins.Add("tan", a => a.convert(b => Math.Tan(Convert.ToDouble(b)), ResultType.Double));
            mapBuiltins.Add("sign", a => a.convert(b => Math.Sign(Convert.ToDouble(b)), ResultType.Double));
        }

        public void builtin(Interpreter intr, Env env)
        {
            env.putValue("builtin", new BuiltinFun(eval) { name = "builtin", limit = false,
                args = new List<string>() { "name", "args" } });

            var code = @"

";
            foreach (var item in code.Split('\n'))
            {
                intr.input(item);
            }
        }

        public Result eval(string name, Result param)
        {
            if (mapBuiltins.ContainsKey(name))
                return mapBuiltins[name](param);
            return Result.Empty;
        }
    }
}
