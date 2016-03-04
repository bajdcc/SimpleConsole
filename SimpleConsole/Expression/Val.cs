using SimpleConsole.Typing;

namespace SimpleConsole.Expression
{
    internal class Val : Expr
    {
        /// <summary>
        /// 变量名
        /// </summary>
        public string ValName { set; get; }

        /// <summary>
        /// 结果
        /// </summary>
        public Result Result { set; get; } = Result.Empty;

        /// <summary>
        /// 可修改
        /// </summary>
        public bool Writable { set; get; } = true;

        public override Result Eval(Env env)
        {
            return Result.IsEmpty ? env.Eval(this, ValName) : Result;
        }

        public override string ToString()
        {
            return ValName ?? Result.ToString();
        }
    }        
}
