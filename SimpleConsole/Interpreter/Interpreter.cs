using SimpleConsole.Expression;
using SimpleConsole.Typing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleConsole
{
    public interface StandardIO
    {
        TextReader IN { set; get; }
        TextWriter OUT { set; get; }
    }

    public interface IInterpreter
    {
        Result input(string input);

        Result input(string input, out string str);

        void addTask(Action task);
    }

    public class Interpreter : IInterpreter, StandardIO
    {
        public TextReader IN { set; get; }
        public TextWriter OUT { set; get; }

        private static Regex rgxMain = new Regex(
            "=>|[-+*/%=\\(\\)\\|]|[A-Za-z_][A-Za-z0-9_]*|(\\d*\\.?\\d+|\\d+\\.?\\d*)([eE][+-]?\\d+)?",
            RegexOptions.Compiled);

        private static Regex rgxVar = new Regex(
            "[A-Za-z_][A-Za-z0-9_]*",
            RegexOptions.Compiled);
        private Env env;
        private List<string> tokens;
        private Builtin builtin;
        private Queue<Action> tasks = new Queue<Action>();

        public Interpreter()
        {
            env = new Env(this);
            builtin = new Builtin(this);
            Console.WriteLine("-----------------------");
            Console.WriteLine("Simple Console - bajdcc");
            Console.WriteLine("-----------------------");
            Console.WriteLine();
            builtin.builtin(this, env);
            Console.WriteLine();
        }

        public Result input(string input)
        {
            tokens = tokenize(input);
            return start();
        }

        public Result input(string input, out string str)
        {
            tokens = tokenize(input);
            Expr exp;
            var result = start(out exp);
            str = exp.ToString();
            return result;
        }

        private List<string> tokenize(string input)
        {
            var tokens = new List<string>();
            MatchCollection matches = rgxMain.Matches(input);
            foreach (Match m in matches)
            {
                tokens.Add(m.Groups[0].Value);
            }
            return tokens;
        }

        private string top()
        {
            if (tokens.Count == 0)
                error("缺少参数");
            return tokens[0];
        }

        private string pop()
        {
            if (tokens.Count == 0)
                error("缺少参数");
            var str = tokens[0];
            tokens.RemoveAt(0);
            return str;
        }

        private string pop(string message)
        {
            if (!available())
                error(message);
            if (tokens.Count == 0)
                error("缺少参数");
            var str = tokens[0];
            tokens.RemoveAt(0);
            return str;
        }

        private bool available()
        {
            return tokens.Count > 0;
        }

        public void addTask(Action task)
        {
            tasks.Enqueue(task);
        }

        private void execTask()
        {
            while (tasks.Count > 0)
            {
                tasks.Dequeue()();
            }
        }

        public void print(Result val)
        {
            OUT.WriteLine(val);
        }

        private IEnumerable<string> takeUntil(string str)
        {
            var l = tokens.TakeWhile(a => a != str).ToList();
            tokens.RemoveRange(0, l.Count());
            return l;
        }

        private Val getValFromToken(string tok)
        {
            long l;
            if (long.TryParse(tok, out l))
            {
                return new Val() { result = new Result() { type = ResultType.Long, val = new List<object>() { l } } };
            }
            double d;
            if (double.TryParse(tok, out d))
            {
                return new Val() { result = new Result() { type = ResultType.Double, val = new List<object>() { d } } };
            }
            return null;
        }

        private void error(string message)
        {
            throw new SCException(message);
        }

        ///////////////////

        private Result start()
        {
            if (!available())
                return Result.Empty;
            env.clear();
            var exp = expr();
            var val = exp.eval(env);
            if (available())
                error("多余参数");
            execTask();
            return val;
        }

        private Result start(out Expr exp)
        {
            if (!available())
                error("输入为空");
            env.clear();
            exp = expr();
            var val = exp.eval(env);
            if (available())
                error("多余参数");
            execTask();
            return val;
        }

        private Expr fn()
        {
            pop();
            var args = takeUntil("=>");
            if (args.Count() == 0)
                error("缺少函数名");
            pop("缺少'=>'");
            if (args.Any(a => !rgxVar.IsMatch(a)))
                error("非法形参");
            var fname = args.First();
            args = args.Skip(1);
            var fun = new Fun() { name = fname, limit = true, args = args, writable = !env.LockVariable };
            env.putValue(fname, fun);
            env.pushNewEnv();
            foreach (var item in args)
            {
                env.putValue(item, new Val() { name = item });
            }
            fun.exp = expr();
            env.popEnv();
            return env.queryValue(fname);
        }

        private Expr fnx()
        {
            pop();
            var args = takeUntil("=>");
            if (args.Count() == 0)
                error("缺少函数名");
            pop("缺少'=>'");
            if (args.Any(a => !rgxVar.IsMatch(a)))
                error("非法形参");
            var fname = args.First();
            args = args.Skip(1);
            if (args.Count() > 2)
                error("必须有一或两个参数");
            var fun = new Fun() { name = fname, limit = false, args = args, writable = !env.LockVariable };
            env.putValue(fname, fun);
            env.pushNewEnv();
            foreach (var item in args)
            {
                env.putValue(item, new Val() { name = item });
            }
            fun.exp = expr();
            env.popEnv();
            return env.queryValue(fname);
        }

        private Expr expr()
        {
            var t = top() == "(" ? brace() : term();
            if (!available() || top() == ")")
                return t;
            var op = oper();
            if (op == OpType.Unknown)
            {
                return t;
            }
            if (op == OpType.Equal && !(t is Val && (t as Val).name != null))
            {
                error("左值必须为变量");
            }
            pop();
            var t2 = expr();
            if (t2 is Binop)
            {
                var tt = t2.GetMostLeftCombineAtom(op, null);
                if (tt != null)
                {
                    var tx = tt as Binop;
                    if (!tx.brace)
                    {
                        tx.op1 = new Binop() { op1 = t, op2 = tx.op1, type = op };
                        return t2;
                    }
                }
            }
            return new Binop() { op1 = t, op2 = t2, type = op };
        }

        private OpType oper()
        {
            if (!available())
                return OpType.Unknown;
            return (OpTypeHelper.GetTypeOfString(top()));
        }
        
        private Expr brace()
        {
            pop();
            var exp = expr();
            if (top() == ")")
                pop();
            if (exp is Binop)
                (exp as Binop).brace = true;
            return exp;
        }

        private Expr term()
        {
            var tok = top();
            if (tok == "fn")
            {
                return fn();
            }
            if (tok == "fnx")
            {
                return fnx();
            }
            pop();
            {
                var v = getValFromToken(tok);
                if (v != null)
                    return v;
            }
            var op = OpTypeHelper.GetTypeOfString(tok);
            if (op != OpType.Unknown)
            {
                if (available())
                {
                    tok += pop();
                    {
                        var v = getValFromToken(tok);
                        if (v != null)
                            return v;
                    }
                }
                error($"非法的操作符'{tok}'");
            }
            var ter = env.queryValueUnsafe(tok);
            if (ter == null)
            {
                if (!available() || top() != "=")
                {
                    if (!char.IsLetter(tok.ToCharArray()[0]))
                    {
                        error($"非法的操作符'{tok}'");
                    }
                    error($"变量'{tok}'未定义");
                }
            }
            if (ter is Val)
            {
                if (available() && top() == "=")
                {
                    if (!(ter as Val).writable)
                        error("变量不可修改");
                }
                return ter;
            }
            else if (ter is Fun)
            {
                if (available() && top() == "=")
                {
                    if (!(ter as Fun).writable)
                        error("函数不可修改");
                    return new Val() { name = tok, writable = !env.LockVariable };
                }
                var fun = ter as Fun;
                var args = new List<Expr>();
                var proc = new Proc() { fun = fun, args = args };
                if (fun.limit)
                {
                    if (fun is BuiltinFun)
                    {
                        var name = pop("缺少参数");
                        if (!rgxVar.IsMatch(name))
                            error("参数必须为变量");
                        args.Add(new Val() { name = name });
                        var count = fun.args.Count();
                        for (int i = 0; i < count - 1; i++)
                        {
                            args.Add(expr());
                        }
                    }
                    else
                    {
                        var count = fun.args.Count();
                        for (int i = 0; i < count; i++)
                        {
                            args.Add(expr());
                        }
                    }
                }
                else
                {
                    if (fun is BuiltinFun || fun is LazyFun)
                    {
                        var name = pop("缺少参数");
                        if (!rgxVar.IsMatch(name))
                            error("参数必须为变量");
                        args.Add(new Val() { name = name });
                    }
                    while (available() && top() != ")")
                    {
                        args.Add(expr());
                    }
                }
                return proc;
            }
            return new Val() { name = tok };
        }
    }
}
