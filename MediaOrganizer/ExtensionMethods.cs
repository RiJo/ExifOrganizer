//
// ExtensionMethods.cs: Static extension methods used within the project.
//
// Copyright (C) 2014 Rikard Johansson
//
// This program is free software: you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the Free
// Software Foundation, either version 3 of the License, or (at your option) any
// later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with
// this program. If not, see http://www.gnu.org/licenses/.
//

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
        public static bool AreFilesIdentical(this FileInfo fileInfo, FileInfo otherFile)
        {
            if (fileInfo == null)
                throw new ArgumentNullException("fileInfo");
            if (fileInfo == null)
                throw new ArgumentNullException("otherFile");
            if (!fileInfo.Exists)
                return false;
            if (!otherFile.Exists)
                return false;

            return fileInfo.GetMD5Sum() == otherFile.GetMD5Sum();
        }

        public static string GetMD5Sum(this FileInfo fileInfo)
        {
            if (fileInfo == null)
                throw new ArgumentNullException("fileInfo");
            if (!fileInfo.Exists)
                throw new FileNotFoundException(fileInfo.FullName);

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
