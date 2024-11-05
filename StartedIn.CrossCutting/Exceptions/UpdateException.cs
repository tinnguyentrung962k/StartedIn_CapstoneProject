using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.Exceptions
{
    public class UpdateException : Exception
    {
        public UpdateException(string message) : base(message)
        {
            
        }
    }
}
