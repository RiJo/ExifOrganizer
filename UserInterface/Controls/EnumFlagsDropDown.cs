//
// EnumFlagsDropDown.cs: User control to render checkboxes, representing flags in 
// Enum tagged with Flags attribute, in a drop-down list.
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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExifOrganizer.UI.Controls
{
    public partial class EnumFlagsDropDown : CheckBoxDropDown
    {
        private Type enumType;
        private Enum enumValue;

        public EnumFlagsDropDown()
        {
            InitializeComponent();
        }

        public Type EnumType
        {
            get { return enumType; }
            set
            {
                if (value == null)
                {
                    enumType = null;
                    Clear();
                    return;
                }

                if (!value.IsEnum)
                    throw new ArgumentException(String.Format("Type must be Enum: {0}", value));
                if (!value.HasAttributesFlags())
                    throw new ArgumentException(String.Format("Enum type must have attribute Flags", value));

                enumType = value;
                enumValue = null;

                foreach (Enum item in Enum.GetValues(value))
                    Add(new CheckBoxItem() { Text = item.ToString() });
            }
        }

        public Enum EnumValue
        {
            get { return enumValue; }
            set
            {
                if (enumType == null)
                    throw new ArgumentException("Enum type not yet defined");
                if (value.GetType() != enumType)
                    throw new ArgumentException(String.Format("Value must be of predefined Enum type: {0}", enumType));

                enumValue = value;
            }
        }
    }

    public static class EnumExtensions
    {
        public static bool Flags(this Enum value)
        {
            Type type = value.GetType();
            return type.HasAttributesFlags();
        }

        public static bool HasAttributesFlags(this Type type)
        {
            object[] attributes = type.GetCustomAttributes(true);
            foreach (object attribute in attributes)
            {
                if (attribute is FlagsAttribute)
                    return true;
            }
            return false;
        }
    }
}
