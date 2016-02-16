using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleConsole
{
    public class SCException : Exception
    {
        public SCException()
        {

        }

        public SCException(string message) : base(message)
        {

        }
    }
}
