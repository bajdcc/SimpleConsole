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
        /// <summary>
        /// 左子表达式
        /// </summary>
        public Expr op1 { set; get; }
        /// <summary>
        /// 右子表达式
        /// </summary>
        public Expr op2 { set; get; }
        /// <summary>
        /// 运算符
        /// </summary>
        public OpType type { set; get; } = OpType.Unknown;
        /// <summary>
        /// 是否为括号表达式
        /// </summary>
        public bool brace { set; get; }

        public override Result eval(Env env)
        {
            switch (type)
            {
                case OpType.Add: return op1.eval(env) + op2.eval(env);
                case OpType.Subtract: return op1.eval(env) - op2.eval(env);
                case OpType.Multiply: return op1.eval(env) * op2.eval(env);
                case OpType.Divide: return op1.eval(env) / op2.eval(env);
                case OpType.Mod: return op1.eval(env) % op2.eval(env);
                case OpType.Equal:
                    var o = op1 as Val;
                    if (!o.writable)
                        throw new SCException($"变量'{o.name}'不可修改");
                    o.result = op2.eval(env);
                    env.putValue(o.name, op1);
                    return o.result;
                default:
                    throw new SCException($"未知运算符{type.GetAttr().Description}");
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
