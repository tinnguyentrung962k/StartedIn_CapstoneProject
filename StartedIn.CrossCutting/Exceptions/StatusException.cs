using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.Exceptions
{
    public class StatusException : Exception
    {
        public StatusException(string message) : base(message) 
        { 
        }
    }
}
