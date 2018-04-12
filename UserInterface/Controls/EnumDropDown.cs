//
// EnumDropDown.cs: User control to render enum values in a drop-down list.
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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ExifOrganizer.UI.Controls
{
    public partial class EnumDropDown : ComboBox
    {
        private Type enumType;
        private Enum enumValue;

        protected class ComboboxItem
        {
            public string Text { get; set; }
            public Enum Value { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }

        public EnumDropDown()
            : base()
        {
            InitializeComponent();

            DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            SelectedIndexChanged += (sender, e) => { enumValue = ((ComboboxItem)SelectedItem).Value; };
        }

        public Func<Enum, string> EnumText
        {
            get;
            set;
        }

        public Type EnumType
        {
            get { return enumType; }
            set
            {
                if (value == null)
                {
                    enumType = null;
                    Items.Clear();
                    return;
                }

                if (!value.IsEnum)
                    throw new ArgumentException($"Type must be Enum: {value}", nameof(value));

                if (value == enumType)
                    return;

                enumType = value;
                enumValue = null;

                Items.Clear();
                foreach (Enum item in Enum.GetValues(value))
                    Items.Add(new ComboboxItem() { Value = item, Text = (EnumText != null) ? EnumText(item) : item.ToString() });
            }
        }

        public Enum EnumValue
        {
            get { return enumValue; }
            set
            {
                if (value == null)
                    return;
                if (enumType == null)
                    throw new ArgumentException("Enum type not yet defined", nameof(enumType));
                if (value.GetType() != enumType)
                    throw new ArgumentException($"Value must be of predefined Enum type: {enumType}", nameof(value));

                if (value == enumValue)
                    return;

                long numeric = value.GetInt64();
                foreach (ComboboxItem item in Items)
                {
                    if (item.Value.GetInt64() != numeric)
                        continue;

                    SelectedItem = item;
                    break;
                }
            }
        }
    }
}