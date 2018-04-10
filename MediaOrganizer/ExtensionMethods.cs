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
using System.IO;
using System.Security.Cryptography;

namespace ExifOrganizer.Organizer
{
    public static class ExtensionMethods
    {
        public static bool FileExistsInDirectory(this FileInfo fileInfo, DirectoryInfo directory, FileComparator comparator)
        {
            foreach (FileInfo tempFile in directory.GetFiles())
            {
                if (tempFile.AreFilesIdentical(fileInfo, comparator))
                    return true;
            }
            return false;
        }

        public static bool AreFilesIdentical(this FileInfo fileInfo, FileInfo otherFile, FileComparator comparator)
        {
            if (fileInfo == null)
                throw new ArgumentNullException(nameof(fileInfo));
            if (otherFile == null)
                throw new ArgumentNullException(nameof(otherFile));
            if (!fileInfo.Exists)
                return false;
            if (!otherFile.Exists)
                return false;

            bool identical = true;
            if (identical && comparator.HasFlag(FileComparator.FileSize))
                identical &= (fileInfo.Length == otherFile.Length);
            if (identical && comparator.HasFlag(FileComparator.ChecksumMD5))
                identical &= fileInfo.GetMD5Sum() == otherFile.GetMD5Sum();
            if (identical && comparator.HasFlag(FileComparator.ChecksumSHA1))
                identical &= fileInfo.GetSHA1Sum() == otherFile.GetSHA1Sum();
            if (identical && comparator.HasFlag(FileComparator.ChecksumSHA256))
                identical &= fileInfo.GetSHA256Sum() == otherFile.GetSHA256Sum();
            if (identical && comparator.HasFlag(FileComparator.Created))
                identical &= (fileInfo.CreationTimeUtc == otherFile.CreationTimeUtc);
            if (identical && comparator.HasFlag(FileComparator.Modified))
                identical &= (fileInfo.LastWriteTimeUtc == otherFile.LastWriteTimeUtc);
            return identical;
        }

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
            {
                using (BufferedStream bufferedStream = new BufferedStream(stream))
                {
                    byte[] checksum = algorithm.ComputeHash(bufferedStream);
                    return BitConverter.ToString(checksum).Replace("-", String.Empty);
                }
            }
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
                throw new ArgumentNullException("value");

            string s = value.ToString().ToLower();
            return !(s == "0" || s == "false");
        }

        public static T ToEnum<T>(this object value) where T : struct
        {
            if (value == null)
                throw new ArgumentNullException("value");

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
    }
}