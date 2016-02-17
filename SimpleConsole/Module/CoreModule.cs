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
            
        }
    }
}
