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

namespace ExifOrganizer.Meta
{
    public static class ExtensionMethods
    {
        //public static string ToString<T>(this Array collection, string separator = ",")
        //{
        //    if (collection == null)
        //        return "<null>";

        //    List<string> values = new List<string>();
        //    foreach (T item in collection)
        //        values.Add(item.ToString());
        //    return String.Join(separator, values.ToArray());
        //}

        //public static string ToString<T>(this T[] collection, string separator = ",")
        //{
        //    if (collection == null)
        //        return "<null>";

        //    List<string> values = new List<string>();
        //    foreach (T item in collection)
        //        values.Add(item.ToString());
        //    return String.Join(separator, values.ToArray());
        //}

        public static string ToString<T>(this IEnumerable<T> collection, string separator = ",")
        {
            if (collection == null)
                return "<null>";

            List<string> values = new List<string>(collection.Count());
            foreach (T item in collection)
                values.Add(item.ToString());
            return String.Join(separator, values.ToArray());
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
    }
}