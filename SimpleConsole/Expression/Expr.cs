using SimpleConsole.Typing;

namespace SimpleConsole.Expression
{
    internal abstract class Expr
    {
        public virtual string Name { get; } = "Expr";

        public virtual Result Eval(Env env)
        {
            return Result.Empty;
        }

        /// <summary>
        /// 返回最左原子子表达式
        /// </summary>
        /// <param name="type"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public virtual Expr GetMostLeftCombineAtom(OpType type, Expr parent)
        {
            return null;
        }
    }
}
