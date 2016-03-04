using SimpleConsole.Expression;
using SimpleConsole.Typing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SimpleConsole
{
    internal interface IStandardIo
    {
        TextReader In { set; get; }
        TextWriter Out { set; get; }
    }

    public interface IInterpreter
    {
        Result Input(string input);

        Result Input(string input, out string str);

        void AddTask(Action task);

        void Print(Result val);
    }

    internal class Interpreter : IInterpreter, IStandardIo
    {
        public TextReader In { set; get; }
        public TextWriter Out { set; get; }

        private static readonly Regex RgxMain = new Regex(
            "=>|[-+*/%=\\(\\)\\|]|[A-Za-z_][A-Za-z0-9_]*|(\\d*\\.?\\d+|\\d+\\.?\\d*)([eE][+-]?\\d+)?",
            RegexOptions.Compiled);

        private static readonly Regex RgxVar = new Regex(
            "[A-Za-z_][A-Za-z0-9_]*",
            RegexOptions.Compiled);
        private readonly Env _env;
        private List<string> _tokens;
        private readonly Queue<Action> _tasks = new Queue<Action>();

        public Interpreter(TextReader tr, TextWriter tw)
        {
            In = tr;
            Out = tw;
            _env = new Env();
            var builtin = new Builtin();
            Out.WriteLine("-----------------------");
            Out.WriteLine("Simple Console - bajdcc");
            Out.WriteLine("-----------------------");
            Out.WriteLine();
            builtin.InitBuiltin(this, this, _env);
            Out.WriteLine();
        }

        public Result Input(string input)
        {
            _tokens = Tokenize(input);
            return Start();
        }

        public Result Input(string input, out string str)
        {
            _tokens = Tokenize(input);
            Expr exp;
            var result = Start(out exp);
            str = exp.ToString();
            return result;
        }

        private List<string> Tokenize(string input)
        {
            return (from Match m in RgxMain.Matches(input) select m.Groups[0].Value).ToList();
        }

        private string Top()
        {
            if (_tokens.Count == 0)
                Error("缺少参数");
            return _tokens[0];
        }

        private string Pop()
        {
            if (_tokens.Count == 0)
                Error("缺少参数");
            var str = _tokens[0];
            _tokens.RemoveAt(0);
            return str;
        }

        private string Pop(string message)
        {
            if (!Available())
                Error(message);
            if (_tokens.Count == 0)
                Error("缺少参数");
            var str = _tokens[0];
            _tokens.RemoveAt(0);
            return str;
        }

        private bool Available()
        {
            return _tokens.Count > 0;
        }

        public void AddTask(Action task)
        {
            _tasks.Enqueue(task);
        }

        private void ExecTask()
        {
            while (_tasks.Count > 0)
            {
                _tasks.Dequeue()();
            }
        }

        public void Print(Result val)
        {
            Out.WriteLine(val);
        }

        private IEnumerable<string> TakeUntil(string str)
        {
            var l = _tokens.TakeWhile(a => a != str).ToList();
            _tokens.RemoveRange(0, l.Count);
            return l;
        }

        private Val GetValFromToken(string tok)
        {
            long l;
            if (long.TryParse(tok, out l))
            {
                return new Val() { Result = new Result() { Type = ResultType.Long, Val = new List<object>() { l } } };
            }
            double d;
            if (double.TryParse(tok, out d))
            {
                return new Val() { Result = new Result() { Type = ResultType.Double, Val = new List<object>() { d } } };
            }
            return null;
        }

        private static void Error(string message)
        {
            throw new ScException(message);
        }

        ///////////////////

        private Result Start()
        {
            if (!Available())
                return Result.Empty;
            _env.Clear();
            var exp = _Expr();
            var val = exp.Eval(_env);
            if (Available())
                Error("多余参数");
            ExecTask();
            return val;
        }

        private Result Start(out Expr exp)
        {
            if (!Available())
                Error("输入为空");
            _env.Clear();
            exp = _Expr();
            var val = exp.Eval(_env);
            if (Available())
                Error("多余参数");
            ExecTask();
            return val;
        }

        private Expr _Fn()
        {
            Pop();
            var args = TakeUntil("=>").ToList();
            if (!args.Any())
                Error("缺少函数名");
            Pop("缺少'=>'");
            if (args.Any(a => !RgxVar.IsMatch(a)))
                Error("非法形参");
            var fname = args.First();
            args.RemoveAt(0);
            var fun = new Fun() { FunName = fname, Limit = true, Args = args, Writable = !_env.LockVariable };
            _env.PutValue(fname, fun);
            _env.PushNewEnv();
            foreach (var item in args)
            {
                _env.PutValue(item, new Val() { ValName = item });
            }
            fun.Exp = _Expr();
            _env.PopEnv();
            return _env.QueryValue(fname);
        }

        private Expr _Fnx()
        {
            Pop();
            var args = TakeUntil("=>").ToList();
            if (!args.Any())
                Error("缺少函数名");
            Pop("缺少'=>'");
            if (args.Any(a => !RgxVar.IsMatch(a)))
                Error("非法形参");
            var fname = args.First();
            args.RemoveAt(0);
            if (args.Count > 2)
                Error("必须有一或两个参数");
            var fun = new Fun() { FunName = fname, Limit = false, Args = args, Writable = !_env.LockVariable };
            _env.PutValue(fname, fun);
            _env.PushNewEnv();
            foreach (var item in args)
            {
                _env.PutValue(item, new Val() { ValName = item });
            }
            fun.Exp = _Expr();
            _env.PopEnv();
            return _env.QueryValue(fname);
        }

        private Expr _Expr()
        {
            var t = Top() == "(" ? _Brace() : _Term();
            if (!Available() || Top() == ")")
                return t;
            var op = _Oper();
            if (op == OpType.Unknown)
            {
                return t;
            }
            if (op == OpType.Equal && (t as Val)?.ValName == null)
            {
                Error("左值必须为变量");
            }
            Pop();
            var t2 = _Expr();
            if (!(t2 is Binop)) return new Binop() {Op1 = t, Op2 = t2, Type = op};
            var tt = t2.GetMostLeftCombineAtom(op, null);
            if (tt == null) return new Binop() {Op1 = t, Op2 = t2, Type = op};
            var tx = tt as Binop;
            if (tx == null) throw new ArgumentNullException(nameof(tx));
            if (tx.Brace) return new Binop() {Op1 = t, Op2 = t2, Type = op};
            tx.Op1 = new Binop() { Op1 = t, Op2 = tx.Op1, Type = op };
            return t2;
        }

        private OpType _Oper()
        {
            return !Available() ? OpType.Unknown : OpTypeHelper.GetTypeOfString(Top());
        }

        private Expr _Brace()
        {
            Pop();
            var exp = _Expr();
            if (Top() == ")")
                Pop();
            if (exp is Binop)
                (exp as Binop).Brace = true;
            return exp;
        }

        private Expr _Term()
        {
            var tok = Top();
            if (tok == "fn")
            {
                return _Fn();
            }
            if (tok == "fnx")
            {
                return _Fnx();
            }
            Pop();
            {
                var v = GetValFromToken(tok);
                if (v != null)
                    return v;
            }
            var op = OpTypeHelper.GetTypeOfString(tok);
            if (op != OpType.Unknown)
            {
                if (Available())
                {
                    tok += Pop();
                    {
                        var v = GetValFromToken(tok);
                        if (v != null)
                            return v;
                    }
                }
                Error($"非法的操作符'{tok}'");
            }
            var ter = _env.QueryValueUnsafe(tok);
            if (ter == null)
            {
                if (!Available() || Top() != "=")
                {
                    if (!char.IsLetter(tok.ToCharArray()[0]))
                    {
                        Error($"非法的操作符'{tok}'");
                    }
                    Error($"变量'{tok}'未定义");
                }
            }
            if (ter is Val)
            {
                if (Available() && Top() == "=")
                {
                    if (!(ter as Val).Writable)
                        Error("变量不可修改");
                }
                return ter;
            }
            else if (ter is Fun)
            {
                if (Available() && Top() == "=")
                {
                    if (!(ter as Fun).Writable)
                        Error("函数不可修改");
                    return new Val() { ValName = tok, Writable = !_env.LockVariable };
                }
                var fun = ter as Fun;
                var args = new List<Expr>();
                var proc = new Proc() { Fun = fun, Args = args };
                if (fun.Limit)
                {
                    if (fun is BuiltinFun)
                    {
                        var name = Pop("缺少参数");
                        if (!RgxVar.IsMatch(name))
                            Error("参数必须为变量");
                        args.Add(new Val() { ValName = name });
                        var count = fun.Args.Count();
                        for (var i = 0; i < count - 1; i++)
                        {
                            args.Add(_Expr());
                        }
                    }
                    else
                    {
                        var count = fun.Args.Count();
                        for (var i = 0; i < count; i++)
                        {
                            args.Add(_Expr());
                        }
                    }
                }
                else
                {
                    if (fun is BuiltinFun || fun is LazyFun)
                    {
                        var name = Pop("缺少参数");
                        if (!RgxVar.IsMatch(name))
                            Error("参数必须为变量");
                        args.Add(new Val() { ValName = name });
                    }
                    while (Available() && Top() != ")")
                    {
                        args.Add(_Expr());
                    }
                }
                return proc;
            }
            return new Val() { ValName = tok };
        }
    }
}
