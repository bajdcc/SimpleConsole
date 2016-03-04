using SimpleConsole.Expression;
using SimpleConsole.Module;
using SimpleConsole.Typing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleConsole
{
    internal class Builtin
    {
        private readonly Dictionary<string, Func<Result, Result>> _mapBuiltins =
            new Dictionary<string, Func<Result, Result>>();
        private readonly Dictionary<string, Tuple<int, Func<IList<Expr>, Env, Result>>> _mapLazys =
            new Dictionary<string, Tuple<int, Func<IList<Expr>, Env, Result>>>();
        private IInterpreter _itpr;
        private Env _env;
        private readonly Dictionary<string, IModule> _mapModules = new Dictionary<string, IModule>();
        private readonly HashSet<string> _loadedModules = new HashSet<string>();

        private void AddBuiltin(string name, Func<Result, Result> func)
        {
            _mapBuiltins.Add(name, func);
        }

        private void AddLazy(string name, int count, Func<IList<Expr>, Env, Result> func)
        {
            _mapLazys.Add(name, new Tuple<int, Func<IList<Expr>, Env, Result>>(count, func));
        }

        private void AddModule(IModule module)
        {
            _mapModules.Add(module.Name, module);
        }

        private void Init()
        {
            InitCore();
            InitMath();
        }

        private void InitCore()
        {
            AddBuiltin("empty", a => a.Parn(0, b => Result.Empty));
            AddBuiltin("bool", a => a.Parn(1, b => new Result() { Val = new List<object> { b.Bool() } }));
            AddBuiltin("not", a => a.Parn(1, b => new Result() { Val = new List<object> { !b.Bool() } }));
            AddBuiltin("is_empty", a => a.Parn(b => new Result() { Val = new List<object>() { b.IsEmpty } }));
            AddBuiltin("is_single", a => a.Parn(b => new Result() { Val = new List<object>() { b.Val.Count() == 1 } }));
            AddBuiltin("is_many", a => a.Parn(b => new Result() { Val = new List<object>() { b.Val.Count() > 1 } }));
            AddBuiltin("equal", a => a.Parn(2, b => b.Compare(CompareType.Equal)));
            AddBuiltin("not_equal", a => a.Parn(2, b => b.Compare(CompareType.NotEqual)));
            AddBuiltin("lt", a => a.Parn(2, b => b.Compare(CompareType.LessThan)));
            AddBuiltin("gt", a => a.Parn(2, b => b.Compare(CompareType.GreaterThan)));
            AddBuiltin("lte", a => a.Parn(2, b => b.Compare(CompareType.NotGreaterThan)));
            AddBuiltin("gte", a => a.Parn(2, b => b.Compare(CompareType.NotLessThan)));
            _mapBuiltins.Add("match", a => a.Parn(3, b => new Result()
            {
                Type = b.Type,
                Val =
                Convert.ToBoolean(b.Val.First()) ? b.Val.Skip(1).Take(1) : b.Val.Skip(2).Take(1)
            }));
            _mapBuiltins.Add("if", a => a.Parn(2, b => new Result()
            {
                Type = b.Type,
                Val =
                Convert.ToBoolean(b.Val.First()) ? b.Val.Skip(1).Take(1) : Result.Empty.Val
            }));
            AddBuiltin("range", a => a.Par2Range());
            AddLazy("match", 3, (a, env) => a[0].Eval(env).Bool() ? a[1].Eval(env) : a[2].Eval(env));
            AddLazy("if", 2, (a, env) => a[0].Eval(env).Bool() ? a[1].Eval(env) : Result.Empty);
            AddLazy("dummy", 1, (a, env) => a[0].Eval(env));
        }

        private void InitMath()
        {
            AddBuiltin("E", a => a.Parn(0, b => new Result() { Type = ResultType.Double, Val = new List<object>() { Math.E } }));
            AddBuiltin("PI", a => a.Parn(0, b => new Result() { Type = ResultType.Double, Val = new List<object>() { Math.PI } }));
            AddBuiltin("square", a => a.Par1(b => Math.Pow(Convert.ToDouble(b), 2.0), ResultType.Double));
            AddBuiltin("sqrt", a => a.Par1(b => Math.Sqrt(Convert.ToDouble(b)), ResultType.Double));
            AddBuiltin("sin", a => a.Par1(b => Math.Sin(Convert.ToDouble(b)), ResultType.Double));
            AddBuiltin("cos", a => a.Par1(b => Math.Cos(Convert.ToDouble(b)), ResultType.Double));
            AddBuiltin("tan", a => a.Par1(b => Math.Tan(Convert.ToDouble(b)), ResultType.Double));
            AddBuiltin("asin", a => a.Par1(b => Math.Asin(Convert.ToDouble(b)), ResultType.Double));
            AddBuiltin("acos", a => a.Par1(b => Math.Acos(Convert.ToDouble(b)), ResultType.Double));
            AddBuiltin("atan", a => a.Par1(b => Math.Atan(Convert.ToDouble(b)), ResultType.Double));
            AddBuiltin("sinh", a => a.Par1(b => Math.Sinh(Convert.ToDouble(b)), ResultType.Double));
            AddBuiltin("cosh", a => a.Par1(b => Math.Cosh(Convert.ToDouble(b)), ResultType.Double));
            AddBuiltin("tanh", a => a.Par1(b => Math.Tanh(Convert.ToDouble(b)), ResultType.Double));
            AddBuiltin("sign", a => a.Par1(b => Math.Sign(Convert.ToDouble(b)), ResultType.Long));
            AddBuiltin("abs", a => a.Par1A(b => Math.Abs(Convert.ToInt64(b)), b => Math.Abs(Convert.ToDouble(b))));
            AddBuiltin("exp", a => a.Par1(b => Math.Exp(Convert.ToDouble(b)), ResultType.Double));
            AddBuiltin("floor", a => a.Par1(b => Math.Floor(Convert.ToDouble(b)), ResultType.Double));
            AddBuiltin("ceiling", a => a.Par1(b => Math.Ceiling(Convert.ToDouble(b)), ResultType.Double));
            AddBuiltin("log", a => a.Par1(b => Math.Log(Convert.ToDouble(b)), ResultType.Double));
            AddBuiltin("log10", a => a.Par1(b => Math.Log10(Convert.ToDouble(b)), ResultType.Double));
            AddBuiltin("round", a => a.Par1(b => Math.Round(Convert.ToDouble(b)), ResultType.Double));
            AddBuiltin("pow", a => a.Par2((x, y) => Math.Pow(Convert.ToDouble(x), Convert.ToDouble(y)), ResultType.Double));
            AddBuiltin("max", a => a.Par2A((x, y) => Math.Max(Convert.ToInt64(x), Convert.ToInt64(y)), (x, y) => Math.Max(Convert.ToDouble(x), Convert.ToDouble(y))));
            AddBuiltin("min", a => a.Par2A((x, y) => Math.Min(Convert.ToInt64(x), Convert.ToInt64(y)), (x, y) => Math.Min(Convert.ToDouble(x), Convert.ToDouble(y))));
            AddBuiltin("sum", a => a.Par2A((x, y) => Convert.ToInt64(x) + Convert.ToInt64(y), (x, y) => Convert.ToDouble(x) + Convert.ToDouble(y)));
            AddBuiltin("product", a => a.Par2A((x, y) => Convert.ToInt64(x) * Convert.ToInt64(y), (x, y) => Convert.ToDouble(x) * Convert.ToDouble(y)));
        }

        public void InitBuiltin(IInterpreter itpr, IStandardIo io, Env env)
        {
            _itpr = itpr;
            _env = env;
            io.Out.WriteLine("Builtin :: Loading...");
            Init();

            var modules = new IModule[]
            {
                new CoreModule(),
                new MathModule(),
            };

            foreach (var item in modules)
            {
                io.Out.WriteLine($"Builtin :: {item.Name}");
                AddModule(item);
            }

            var builtinFuncs = new Fun[]
            {
                new BuiltinFun(EvalBuiltin)
                {
                    FunName = "builtin",
                    Limit = false,
                    Args = new List<string>() { "name", "args" }
                },
                new BuiltinFun(EvalType)
                {
                    FunName = "type",
                    Limit = true,
                    Args = new List<string>() { "t" }
                },
                new BuiltinFun(EvalLoad)
                {
                    FunName = "load",
                    Limit = true,
                    Args = new List<string>() { "name" }
                },
                new LazyFun(EvalLazy)
                {
                    FunName = "lazy"
                },
            };
            foreach (var item in builtinFuncs)
            {
                item.RegisterToEnv(env);
            }

            var code = @"
load Core
";
            env.LockVariable = true;
            foreach (var item in code.Split('\n'))
            {
                itpr.Input(item);
            }
            env.LockVariable = false;

            io.Out.WriteLine("Builtin :: OK");
        }

        private Result EvalBuiltin(string name, Result param)
        {
            if (_mapBuiltins.ContainsKey(name))
                return _mapBuiltins[name](param);
            throw new ScException($"'{name}' not found.");
        }

        private Result EvalType(string name, Result param)
        {
            var v = _env.QueryValueAll(name);
            if (v == null)
                return new StringResult($"'{name}' not found.");
            if (v is Val || v is Binop)
                return new StringResult(v.Eval(_env).GetTypeString());
            if (v is Fun)
                return new StringResult(v.Name);
            return Result.Empty;
        }

        private Result EvalLoad(string name, Result param)
        {
            if (_mapModules.ContainsKey(name))
            {
                if (_loadedModules.Contains(name))
                    return new StringResult($"Module :: {name} has been loaded.");
                _loadedModules.Add(name);
                _itpr.AddTask(() => _mapModules[name].Load(_itpr, _env));
                return new StringResult($"Module :: {name} loaded.");
            }
            return new StringResult($"Module :: {name} not found.");
        }

        private Result EvalLazy(string name, IList<Expr> exps, Env env)
        {
            if (!_mapLazys.ContainsKey(name)) return new StringResult($"'{name}' not found.");
            var lazy = _mapLazys[name];
            if (lazy.Item1 >= 0 && exps.Count != lazy.Item1)
                throw new ScException($"需要{lazy.Item1}个参数");
            return lazy.Item2(exps, env);
        }
    }
}
