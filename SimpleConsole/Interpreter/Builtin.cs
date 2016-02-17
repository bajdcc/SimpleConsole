using SimpleConsole.Expression;
using SimpleConsole.Module;
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
        private IInterpreter itpr;
        private Env env;
        private StandardIO IO;
        private Dictionary<string, IModule> mapModules = new Dictionary<string, IModule>();
        private HashSet<string> loadedModules = new HashSet<string>();

        public Builtin(StandardIO io)
        {
            IO = io;
        }

        private void init()
        {
            initCore();
            initMath();
        }

        private void initCore()
        {
            Console.WriteLine("Builtin :: Core");
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
            mapBuiltins.Add("min", a => a.par2a((x, y) => Math.Min(Convert.ToInt64(x), Convert.ToInt64(y)), (x, y) => Math.Min(Convert.ToDouble(x), Convert.ToDouble(y))));
            mapBuiltins.Add("sum", a => a.par2a((x, y) => Convert.ToInt64(x) + Convert.ToInt64(y), (x, y) => Convert.ToDouble(x) + Convert.ToDouble(y)));
            mapBuiltins.Add("product", a => a.par2a((x, y) => Convert.ToInt64(x) * Convert.ToInt64(y), (x, y) => Convert.ToDouble(x) * Convert.ToDouble(y)));
            Console.WriteLine("Builtin :: Math");
        }

        public void builtin(IInterpreter itpr, Env env)
        {
            this.itpr = itpr;
            this.env = env;
            Console.WriteLine("Builtin :: Loading...");
            init();
            mapModules.Add("Math", new MathModule());

            env.putValue("builtin", new BuiltinFun(evalBuiltin)
            {
                name = "builtin",
                limit = false,
                args = new List<string>() { "name", "args" }
            });
            env.putValue("type", new BuiltinFun(evalType)
            {
                name = "type",
                limit = false,
                args = new List<string>() { "name", "args" }
            });
            env.putValue("load", new BuiltinFun(evalLoad)
            {
                name = "load",
                limit = false,
                args = new List<string>() { "name", "args" }
            });

            var code = @"
";
            env.LockVariable = true;
            foreach (var item in code.Split('\n'))
            {
                itpr.input(item);
            }
            env.LockVariable = false;
            Console.WriteLine("Builtin :: OK");
        }

        public Result evalBuiltin(string name, Result param)
        {
            if (mapBuiltins.ContainsKey(name))
                return mapBuiltins[name](param);
            throw new SCException($"'{name}' not found.");
        }

        public Result evalType(string name, Result param)
        {
            var v = env.queryValueAll(name);
            if (v == null)
                return new StringResult($"'{name}' not found.");
            if (v is Val || v is Binop)
                return new StringResult(v.eval(env));
            if (v is Fun)
                return new StringResult(v.Name);
            return Result.Empty;
        }

        public Result evalLoad(string name, Result param)
        {
            if (mapModules.ContainsKey(name))
            {
                if (loadedModules.Contains(name))
                    return new StringResult($"Module :: {name} has been loaded.");
                loadedModules.Add(name);
                itpr.addTask(() => mapModules[name].load(itpr, env));
                return new StringResult($"Module :: {name} loaded.");
            }
            return new StringResult($"Module :: {name} not found.");
        }
    }
}
