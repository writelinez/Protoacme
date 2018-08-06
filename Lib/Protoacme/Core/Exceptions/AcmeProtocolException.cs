using System;
using System.Collections.Generic;
using System.Text;

namespace Protoacme.Core.Exceptions
{
    public class AcmeProtocolException : Exception
    {
        public AcmeProtocolException()
            :base()
        { }

        public AcmeProtocolException(string message)
            : base(message)
        { }
    }
}
