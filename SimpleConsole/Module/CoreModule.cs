using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleConsole.Module
{
    class CoreModule : IModule
    {
        public string Name
        {
            get
            {
                return "Core";
            }
        }

        public void load(IInterpreter itpr, Env env)
        {
            var code = @"
empty = builtin empty
fnx head x xs => x
fnx tail x xs => xs
fnx list x => x
fn bool x => builtin bool x
fn not x => builtin not x
fnx is_empty x => builtin is_empty x
fnx is_single x => builtin is_single x
fnx is_many x => builtin is_many x
fn match cond t f => builtin match cond t f
fn if cond t => builtin if cond t
fn exec exp => builtin exec exp
fn equal x y => builtin equal x y
fn not_equal x y => builtin not_equal x y
fn lt x y => builtin lt x y
fn gt x y => builtin gt x y
fn lte x y => builtin lte x y
fn gte x y => builtin gte x y
fn range x y => builtin range x y
";
            env.LockVariable = true;
            foreach (var item in code.Split('\n'))
            {
                itpr.input(item);
            }
            env.LockVariable = false;
        }
    }
}
