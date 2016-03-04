using System.Collections.Generic;
using System.Linq;
using SimpleConsole.Expression;
using SimpleConsole.Typing;

namespace SimpleConsole
{
    internal class Env
    {
        private const int StackDepth = 500;
        private readonly List<Dictionary<string, Expr>> _envStack = new List<Dictionary<string, Expr>>();

        public Env()
        {
            PushNewEnv();
        }

        public bool LockVariable { set; get; }

        public void PushNewEnv()
        {
            if (_envStack.Count > StackDepth)
                throw new ScException("堆栈溢出");
            _envStack.Insert(0, new Dictionary<string, Expr>());
        }

        public void PopEnv()
        {
            _envStack.RemoveAt(0);
        }

        public bool IsTopEnv()
        {
            return _envStack.Count == 1;
        }

        public void PutValue(string name, Expr exp)
        {
            if (_envStack[0].ContainsKey(name))
                _envStack[0][name] = exp;
            else
                _envStack[0].Add(name, exp);
        }

        public Expr QueryValue(string name)
        {
            if (_envStack[0].ContainsKey(name))
                return _envStack[0][name];
            if (_envStack[_envStack.Count - 1].ContainsKey(name))
            {
                var exp = _envStack[_envStack.Count - 1][name];
                if (exp is Fun)
                    return exp;
            }
            throw new ScException($"变量'{name}'不存在");
        }

        public Expr QueryValueUnsafe(string name)
        {
            if (_envStack[0].ContainsKey(name))
                return _envStack[0][name];
            if (_envStack[_envStack.Count - 1].ContainsKey(name))
            {
                var exp = _envStack[_envStack.Count - 1][name];
                if (exp is Fun)
                    return exp;
            }
            return null;
        }

        public Result Eval(Expr caller, string name)
        {
            if (name == null)
                return Result.Empty;
            var v = QueryValue(name);
            if (v == caller)
                return Result.Empty;
            return v.Eval(this);
        }

        public Expr QueryValueAll(string name)
        {
            foreach (var item in _envStack.Where(item => item.ContainsKey(name)))
            {
                return item[name];
            }
            throw new ScException("变量不存在");
        }

        public void Clear()
        {
            if (_envStack.Count > 1)
                _envStack.RemoveRange(0, _envStack.Count - 1);
        }

        public override string ToString()
        {
            return string.Join(" ", _envStack[0].Keys.OrderBy(a => a));
        }
    }
}