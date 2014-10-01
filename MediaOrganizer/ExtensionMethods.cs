using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ExifOrganizer.Organizer
{
    public static class ExtensionMethods
    {
        public static string GetMD5Sum(this FileInfo fileInfo)
        {
            if (fileInfo == null)
                throw new ArgumentNullException("fileInfo");
            if (!fileInfo.Exists)
                throw new FileNotFoundException(String.Format("Cannot locate file to calculate md5sum: {0}", fileInfo.FullName));

            string filename = fileInfo.FullName;
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
