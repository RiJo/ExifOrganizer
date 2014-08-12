using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ExifOrganizer.Meta.Parsers
{
    internal class Parser
    {
        protected static long GetFileSize(string filename)
        {
            return new FileInfo(filename).Length;
        }
    }
}
