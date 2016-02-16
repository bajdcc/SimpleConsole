using SimpleConsole.Typing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleConsole.Expression
{
    class Binop : Expr
    {
        public Expr op1 { set; get; }
        public Expr op2 { set; get; }
        public OpType type { set; get; }
        public bool brace { set; get; }
        public override double eval(Env env)
        {
            switch (type)
            {
                case OpType.Add: return op1.eval(env) + op2.eval(env);
                case OpType.Subtract: return op1.eval(env) - op2.eval(env);
                case OpType.Multiply: return op1.eval(env) * op2.eval(env);
                case OpType.Divide: return op1.eval(env) / op2.eval(env);
                case OpType.Mod: return op1.eval(env) % op2.eval(env);
                case OpType.Equal:
                    if (!(op1 is Val))
                        return 0.0;
                    var o = op1 as Val;
                    o.val = op2.eval(env);
                    env.putValue(o.name, op1);
                    return o.val ?? 0.0;
                default:
                    return 0.0;
            }
        }
        private static string getOpDesc(OpType op)
        {
            return op.GetAttr().Description;
        }
        public override string ToString()
        {
            return $"({op1} {getOpDesc(type)} {op2})";
        }
    }
}
