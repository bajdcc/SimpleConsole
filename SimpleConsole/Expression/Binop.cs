using System;
using SimpleConsole.Typing;

namespace SimpleConsole.Expression
{
    internal class Binop : Expr
    {
        /// <summary>
        /// 左子表达式
        /// </summary>
        public Expr Op1 { set; get; }
        /// <summary>
        /// 右子表达式
        /// </summary>
        public Expr Op2 { set; get; }
        /// <summary>
        /// 运算符
        /// </summary>
        public OpType Type { set; get; } = OpType.Unknown;
        /// <summary>
        /// 是否为括号表达式
        /// </summary>
        public bool Brace { set; get; }

        public override Result Eval(Env env)
        {
            switch (Type)
            {
                case OpType.Match:
                case OpType.Add: return Op1.Eval(env) + Op2.Eval(env);
                case OpType.Subtract: return Op1.Eval(env) - Op2.Eval(env);
                case OpType.Multiply: return Op1.Eval(env) * Op2.Eval(env);
                case OpType.Divide: return Op1.Eval(env) / Op2.Eval(env);
                case OpType.Mod: return Op1.Eval(env) % Op2.Eval(env);
                case OpType.Equal:
                    var o = Op1 as Val;
                    if (o == null)
                        throw new NullReferenceException(nameof(o));
                    if (!o.Writable)
                        throw new ScException($"变量'{o.ValName}'不可修改");
                    o.Result = Op2.Eval(env);
                    env.PutValue(o.ValName, Op1);
                    return o.Result;
                default:
                    throw new ScException($"未知运算符{Type.GetAttr().Description}");
            }
        }

        private static string GetOpDesc(OpType op)
        {
            return op.GetAttr().Description;
        }

        /// <summary>
        /// 递归查找，自下向上返回
        /// </summary>
        /// <param name="type"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public override Expr GetMostLeftCombineAtom(OpType type, Expr parent)
        {
            if (Brace)
                return parent ?? this;
            var atom = Op1.GetMostLeftCombineAtom(type, this);
            if (atom == null)
                return type.GetAttr().LeftLevel >= Type.GetAttr().RightLevel ? (parent ?? this) : null;
            return atom;
        }

        public override string ToString()
        {
            return $"({Op1} {GetOpDesc(Type)} {Op2})";
        }
    }
}
