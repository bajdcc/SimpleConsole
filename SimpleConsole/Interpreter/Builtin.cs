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
            mapBuiltins.Add("sin", a => a.par1(b => Math.Sin(Convert.ToDouble(b)), ResultType.Double));
            mapBuiltins.Add("cos", a => a.par1(b => Math.Cos(Convert.ToDouble(b)), ResultType.Double));
            mapBuiltins.Add("tan", a => a.par1(b => Math.Tan(Convert.ToDouble(b)), ResultType.Double));
            mapBuiltins.Add("asin", a => a.par1(b => Math.Asin(Convert.ToDouble(b)), ResultType.Double));
            mapBuiltins.Add("acos", a => a.par1(b => Math.Acos(Convert.ToDouble(b)), ResultType.Double));
            mapBuiltins.Add("atan", a => a.par1(b => Math.Atan(Convert.ToDouble(b)), ResultType.Double));
            mapBuiltins.Add("sinh", a => a.par1(b => Math.Sinh(Convert.ToDouble(b)), ResultType.Double));
            mapBuiltins.Add("cosh", a => a.par1(b => Math.Cosh(Convert.ToDouble(b)), ResultType.Double));
            mapBuiltins.Add("tanh", a => a.par1(b => Math.Tanh(Convert.ToDouble(b)), ResultType.Double));
            mapBuiltins.Add("sign", a => a.par1(b => Math.Sign(Convert.ToDouble(b)), ResultType.Long));
            mapBuiltins.Add("abs", a => a.par1a(b => Math.Abs(Convert.ToInt64(b)), b => Math.Abs(Convert.ToDouble(b))));
            mapBuiltins.Add("exp", a => a.par1(b => Math.Exp(Convert.ToDouble(b)), ResultType.Double));
            mapBuiltins.Add("floor", a => a.par1(b => Math.Floor(Convert.ToDouble(b)), ResultType.Double));
            mapBuiltins.Add("ceiling", a => a.par1(b => Math.Ceiling(Convert.ToDouble(b)), ResultType.Double));
            mapBuiltins.Add("log", a => a.par1(b => Math.Log(Convert.ToDouble(b)), ResultType.Double));
            mapBuiltins.Add("log10", a => a.par1(b => Math.Log10(Convert.ToDouble(b)), ResultType.Double));
            mapBuiltins.Add("round", a => a.par1(b => Math.Round(Convert.ToDouble(b)), ResultType.Double));
            mapBuiltins.Add("pow", a => a.par2((x, y) => Math.Pow(Convert.ToDouble(x), Convert.ToDouble(y)), ResultType.Double));
            mapBuiltins.Add("max", a => a.par2a((x, y) => Math.Max(Convert.ToInt64(x), Convert.ToInt64(y)), (x, y) => Math.Max(Convert.ToDouble(x), Convert.ToDouble(y))));
        }

        public void builtin(Interpreter intr, Env env)
        {
            Console.WriteLine("Builtin :: Loading...");
            env.putValue("builtin", new BuiltinFun(eval) { name = "builtin", limit = false,
                args = new List<string>() { "name", "args" } });

            var code = @"

";
            foreach (var item in code.Split('\n'))
            {
                intr.input(item);
            }
            Console.WriteLine("Builtin :: OK");
        }

        public Result eval(string name, Result param)
        {
            if (mapBuiltins.ContainsKey(name))
                return mapBuiltins[name](param);
            return Result.Empty;
        }
    }
}
