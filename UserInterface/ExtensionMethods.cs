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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExifOrganizer.UI
{
    public static class ExtensionMethods
    {
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
                throw new ArgumentNullException("type");

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
    }
}
