//
// ExtensionMethods.cs: Static extension methods used within the project.
//
// Copyright (C) 2018 Rikard Johansson
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

namespace ExifOrganizer.Common
{
    public static class ExtensionMethods
    {
        public static string SuffixFileName(this FileInfo fileInfo, int index)
        {
            if (fileInfo == null)
                throw new ArgumentNullException(nameof(fileInfo));
            if (index < 0)
                throw new ArgumentException(nameof(index));

            string path = fileInfo.DirectoryName;
            string fileName = Path.GetFileNameWithoutExtension(fileInfo.FullName);
            string extension = Path.GetExtension(fileInfo.FullName);

            return $"{path}\\{fileName}({index}){extension}";
        }

        public static string ReplaceInvalidPathChars(this string path, char replacement = '_')
        {
            foreach (char invalidChar in Path.GetInvalidPathChars())
                path = path.Replace(invalidChar, replacement);
            return path;
        }

        public static bool ToBool(this object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            string s = value.ToString().ToLower();
            return !(s == "0" || s == "false");
        }

        public static T ToEnum<T>(this object value) where T : struct
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            string s = value.ToString();
            long i;
            if (long.TryParse(s, out i))
            {
                // Interpret as numeric enum value
                return (T)(object)i;
            }
            else
            {
                // Interpret as string enum name
                return (T)Enum.Parse(typeof(T), s);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
            {
                action(item);
            }
        }

        public static string ToString<T>(this IEnumerable<T> enumeration, string separator = ",")
        {
            if (enumeration == null)
                return "<null>";

            return String.Join(separator, enumeration);
        }

        public static string RemoveNullChars(this string s)
        {
            return s.Replace("\0", "");
        }

        public static string UppercaseFirst(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }

        public static bool DirectoryAreSame(this string rootPath, string subPath)
        {
            DirectoryInfo rootDir = new DirectoryInfo(rootPath);
            DirectoryInfo subDir = new DirectoryInfo(subPath);
            return rootDir.FullName == subDir.FullName;
        }

        public static bool DirectoryIsSubPath(this string rootPath, string subPath, bool includeRootPath = true)
        {
            DirectoryInfo rootDir = new DirectoryInfo(rootPath);
            DirectoryInfo subDir = new DirectoryInfo(subPath);

            if (includeRootPath && rootDir.FullName == subDir.FullName)
                return true;

            bool isParent = false;
            while (subDir.Parent != null)
            {
                if (subDir.Parent.FullName == rootDir.FullName)
                {
                    isParent = true;
                    break;
                }
                else
                {
                    subDir = subDir.Parent;
                }
            }

            return isParent;
        }

        public static string GetMD5Sum(this FileInfo fileInfo)
        {
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
                return fileInfo.CalculateChecksum(md5);
        }

        public static string GetSHA1Sum(this FileInfo fileInfo)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
                return fileInfo.CalculateChecksum(sha1);
        }

        public static string GetSHA256Sum(this FileInfo fileInfo)
        {
            using (SHA256Managed sha256 = new SHA256Managed())
                return fileInfo.CalculateChecksum(sha256);
        }

        private static string CalculateChecksum(this FileInfo fileInfo, HashAlgorithm algorithm)
        {
            if (fileInfo == null)
                throw new ArgumentNullException(nameof(fileInfo));
            if (!fileInfo.Exists)
                throw new FileNotFoundException(fileInfo.FullName);

            using (FileStream stream = File.OpenRead(fileInfo.FullName))
                return stream.CalculateChecksum(algorithm);
        }

        public static string GetMD5Sum(this FileStream fileStream)
        {
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
                return fileStream.CalculateChecksum(md5);
        }

        public static string GetSHA1Sum(this FileStream fileStream)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
                return fileStream.CalculateChecksum(sha1);
        }

        public static string GetSHA256Sum(this FileStream fileStream)
        {
            using (SHA256Managed sha256 = new SHA256Managed())
                return fileStream.CalculateChecksum(sha256);
        }

        private static string CalculateChecksum(this FileStream fileStream, HashAlgorithm algorithm)
        {
            if (fileStream == null)
                throw new ArgumentNullException(nameof(fileStream));

            using (BufferedStream bufferedStream = new BufferedStream(fileStream))
            {
                byte[] checksum = algorithm.ComputeHash(bufferedStream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
        }
    }
}