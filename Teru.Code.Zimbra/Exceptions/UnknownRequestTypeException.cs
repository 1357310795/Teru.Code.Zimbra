using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teru.Code.Zimbra.Exceptions
{
    internal class UnknownRequestTypeException : Exception
    {
        public UnknownRequestTypeException()
        {
        }

        public UnknownRequestTypeException(string? message) : base(message)
        {
        }
    }
}
