using SimpleConsole.Expression;
using SimpleConsole.Typing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleConsole
{
    class Env
    {
        private const int STACK_DEPTH = 1000;
        private List<Dictionary<string, Expr>> envStack = new List<Dictionary<string, Expr>>();
        private StandardIO IO; 

        public bool LockVariable { set; get; } = false;
        public bool LookAheadFunc { set; get; } = true;

        public Env(StandardIO io)
        {
            IO = io;
            pushNewEnv();
        }

        public void pushNewEnv()
        {
            if (envStack.Count > STACK_DEPTH)
                throw new SCException("堆栈溢出");
            envStack.Insert(0, new Dictionary<string, Expr>());
        }

        public void popEnv()
        {
            envStack.RemoveAt(0);
        }

        public void putValue(string name, Expr exp)
        {
            if (envStack[0].ContainsKey(name))
                envStack[0][name] = exp;
            else
                envStack[0].Add(name, exp);
        }

        public Expr queryValue(string name)
        {
            if (envStack[0].ContainsKey(name))
                return envStack[0][name];
            if (envStack[envStack.Count - 1].ContainsKey(name))
            {
                var exp = envStack[envStack.Count - 1][name];
                if (exp is Fun)
                    return exp;
            }
            return null;
        }

        public Result eval(string name)
        {
            if (name == null)
                return Result.Empty;
            return queryValue(name).eval(this);
        }

        public Expr queryValueAll(string name)
        {
            foreach (var item in envStack)
            {
                if (item.ContainsKey(name))
                    return item[name];
            }
            throw new SCException("变量不存在");
        }

        public void clear()
        {
            if (envStack.Count > 1)
                envStack.RemoveRange(0, envStack.Count - 1);
        }

        public override string ToString()
        {
            return string.Join(" ", envStack[0].Keys.OrderBy(a => a));
        }
    }
}
