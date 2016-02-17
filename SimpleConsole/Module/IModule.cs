using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleConsole.Module
{
    interface IModule
    {
        void load(IInterpreter itpr, Env env);

        string Name { get; }
    }
}
