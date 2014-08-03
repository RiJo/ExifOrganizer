using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaParser
{
    public class MediaOrganizerException : Exception
    {
        public MediaOrganizerException(string message, params object[] args)
            : base(String.Format(message, args))
        {
        }

        public MediaOrganizerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
