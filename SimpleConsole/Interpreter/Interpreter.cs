using SimpleConsole.Expression;
using SimpleConsole.Typing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleConsole
{
    public class Interpreter
    {
        private static Regex rgxMain = new Regex(
            "=>|[-+*/%=\\(\\)]|[A-Za-z_][A-Za-z0-9_]*|\\d*(?:\\.?\\d+)",
            RegexOptions.Compiled);
        private Env env = new Env();
        private List<string> tokens;

        public double? input(string input)
        {
            tokens = tokenize(input);
            return start();
        }

        public double? input(string input, out string str)
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
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(string.Join(" ", tokens));
            return tokens;
        }

        private string top()
        {
            if (tokens.Count == 0)
                error("缺少参数");
            return tokens.First();
        }

        private string pop()
        {
            if (tokens.Count == 0)
                error("缺少参数");
            var str = tokens.First();
            tokens.RemoveAt(0);
            return str;
        }

        private bool available()
        {
            return tokens.Count > 0;
        }

        private List<string> takeUntil(string str)
        {
            var l = tokens.TakeWhile(a => a != str).ToList();
            tokens.RemoveRange(0, l.Count);
            return l;
        }

        private void error(string message)
        {
            throw new SCException(message);
        }

        ///////////////////

        private double? start()
        {
            env.clear();
            var exp = expr();
            var val = exp.eval(env);
            if (available())
                error("多余参数");
            return val;
        }

        private double? start(out Expr exp)
        {
            env.clear();
            exp = expr();
            var val = exp.eval(env);
            if (available())
                error("多余参数");
            return val;
        }

        private Expr fn()
        {
            pop();
            var args = takeUntil("=>");
            pop();
            var fname = args[0];
            args.RemoveAt(0);
            env.pushNewEnv();
            foreach (var item in args)
            {
                env.putValue(item, new Val() { name = item });
            }
            var exp = expr();
            env.popEnv();
            env.putValue(fname, new Fun() { name = fname, args = args, exp = exp });
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
                var tt = t2 as Binop;
                var tx = tt;
                // 找到tt的最左子结点的父结点tx
                // 要求：若途中binop不可再分，即brace=true，则中止
                // 要求：若左结合优先级>=右结合优先级，则置换
                while (!tx.brace && tx.op1 is Binop && op.GetAttr().LeftLevel < tx.type.GetAttr().RightLevel)
                {
                    tx = tt.op1 as Binop;
                }
                if (tx.brace || op.GetAttr().LeftLevel >= tx.type.GetAttr().RightLevel)
                {
                    tx.op1 = new Binop() { op1 = t, op2 = tx.op1, type = op };
                    return tt;
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
            pop();
            double val;
            if (double.TryParse(tok, out val))
            {
                return new Val() { val = val };
            }
            var op = OpTypeHelper.GetTypeOfString(tok);
            if (op != OpType.Unknown)
            {
                if (double.TryParse(tok + top(), out val))
                {
                    pop();
                    return new Val() { val = val };
                }
                error($"非法的操作符'{tok}'");
            }
            var ter = env.queryValue(tok);
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
                return ter;
            }
            else if (ter is Fun)
            {
                if (available() && top() == "=")
                {
                    return new Val() { name = tok };
                }
                var fun = ter as Fun;
                var proc = new Proc() { fun = fun, args = new List<Expr>() };
                for (int i = 0; i < fun.args.Count; i++)
                {
                    proc.args.Add(expr());
                }
                return proc;
            }
            return new Val() { name = tok };
        }
    }
}
