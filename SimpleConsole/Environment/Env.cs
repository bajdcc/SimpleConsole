using SimpleConsole.Expression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleConsole
{
    class Env
    {
        private List<Dictionary<string, Expr>> envStack = new List<Dictionary<string, Expr>>();

        public Env()
        {
            pushNewEnv();
        }

        public void pushNewEnv()
        {
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
            return null;
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
