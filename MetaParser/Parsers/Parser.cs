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

        protected static string GetMD5Sum(string filename)
        {
            using (FileStream stream = File.OpenRead(filename))
            {
                using (var bufferedStream = new BufferedStream(stream, 1024 * 32))
                {
                    var sha = new MD5CryptoServiceProvider();
                    byte[] checksum = sha.ComputeHash(bufferedStream);
                    return BitConverter.ToString(checksum).Replace("-", String.Empty);
                }
            }
        }
    }
}
