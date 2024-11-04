using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.Exceptions
{
    public class DownloadErrorException : Exception
    {
        public DownloadErrorException(string message) : base(message)
        {

        }
    }
}
