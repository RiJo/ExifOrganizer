using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaParser
{
    public class MetaParseException : Exception
    {
        public MetaParseException(string message, params object[] args)
            : base(String.Format(message, args))
        {
        }

        public MetaParseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
