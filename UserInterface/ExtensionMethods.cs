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

using ExifOrganizer.Organizer;
using System;
using System.Windows.Forms;

namespace ExifOrganizer.UI
{
    public static class ExtensionMethods
    {
        #region Generic

        public static int GetInt32(this Enum value)
        {
            if (value == null)
                return 0;
            return Convert.ToInt32(value);
        }

        public static long GetInt64(this Enum value)
        {
            if (value == null)
                return 0;
            return Convert.ToInt64(value);
        }

        public static bool Flags(this Enum value)
        {
            if (value == null)
                return false;

            Type type = value.GetType();
            return type.HasAttributesFlags();
        }

        public static bool HasAttributesFlags(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            object[] attributes = type.GetCustomAttributes(true);
            foreach (object attribute in attributes)
            {
                if (attribute is FlagsAttribute)
                    return true;
            }
            return false;
        }

        public static bool OneBitSet(this Enum value)
        {
            long x = value.GetInt64();
            return (x != 0) && (x & (x - 1)) == 0;
        }

        public static double Clamp(this double value, double min, double max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        public static Exception GetInnerMost(this Exception exception)
        {
            if (exception == null)
                return null;
            if (exception.InnerException == null)
                return exception;

            return exception.InnerException.GetInnerMost();
        }

        public static string Get(this Version version)
        {
            return $"{version.Major}.{version.Minor}.{version.Build} build {version.Revision}";
        }

        #endregion Generic

        #region WinForms

        public static void Invoke(this Control control, Action action, params object[] args)
        {
            control.Invoke((Delegate)action, args);
        }

        public static object Invoke(this Control control, Func<object> func, params object[] args)
        {
            return control.Invoke((Delegate)func, args);
        }

        public static IAsyncResult BeginInvoke(this Control control, Action action, params object[] args)
        {
            return control.BeginInvoke((Delegate)action, args);
        }

        #endregion WinForms

        #region MediaOrganizer

        //        public static string ToJSON(this OrganizeSummary)
        //        {
        //            return String.Format("Summary {{ Parsed: {0} (Ignored: {1}), Total files: {2}, Total directories: {3} }}",
        //    parsed != null ? parsed.Length : 0,
        //    ignored != null ? ignored.Length : 0,
        //    totalFiles != null ? totalFiles.Length : 0,
        //    totalDirectories != null ? totalDirectories.Length : 0
        //);
        //        }

        #endregion MediaOrganizer
    }
}