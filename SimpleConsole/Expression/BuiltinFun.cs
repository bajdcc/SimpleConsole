﻿using SimpleConsole.Typing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleConsole.Expression
{
    class BuiltinFun : Fun
    {
        private Func<string, Result, Result> evalBuiltin;

        public BuiltinFun(Func<string, Result, Result> func)
        {
            evalBuiltin = func;
            limit = false;
            writable = false;
        }

        public override Result eval(Env env)
        {
            var x = env.queryValue(args[0]) as Val;
            var xs = env.queryValue(args[1]) as Val;
            return evalBuiltin(x.name, xs.result);
        }

        public override string ToString()
        {
            return $"builtin {name}{string.Concat(args.Select(a => " " + a))}";
        }
    }
}
