using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teru.Code.Zimbra.Exceptions
{
    public class RequestHeaderContextException : Exception
    {
        public RequestHeaderContextException()
        {
        }

        public RequestHeaderContextException(string? message) : base(message)
        {
        }
    }
}
