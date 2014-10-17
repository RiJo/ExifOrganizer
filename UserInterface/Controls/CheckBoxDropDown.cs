//
// CheckBoxDropDown.cs: User control to render checkboxes in a drop-down list.
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
    public class CheckBoxItem
    {
        public int Value;
        public string Text;
        public bool Checked;
    }

    public partial class CheckBoxDropDown : ComboBox
    {
        private ToolStripDropDownMenu popup;
        private List<CheckBoxItem> items = new List<CheckBoxItem>();

        private class ToolStripCheckboxItem : ToolStripControlHost
        {
            public event Action<object, EventArgs> CheckedChanged;
            //public event Action<object, EventArgs> CheckStateChanged;

            private CheckBoxItem item;
            private CheckBox checkbox;

            public CheckBoxItem Item { get { return item; } }
            public CheckBox CheckBox { get { return checkbox; } }
            public bool Checked { get { return checkbox.Checked; } }
            public CheckState CheckState { get { return checkbox.CheckState; } }

            //public ToolStripCheckboxItem(string text, CheckState state = CheckState.Unchecked)
            //{

            //}

            public ToolStripCheckboxItem(CheckBoxItem foo)
                : base(new CheckBox())
            {
                item = foo;

                checkbox = Control as CheckBox;
                checkbox.Checked = item.Checked;
                checkbox.Text = item.Text;
                checkbox.AutoSize = true;
                checkbox.CheckedChanged += delegate(object sender, EventArgs e)
                {
                    item.Checked = checkbox.Checked;

                    if (CheckedChanged != null)
                        CheckedChanged(this, e);
                };
                //checkbox.CheckStateChanged += delegate(object sender, EventArgs e)
                //{
                //    if (CheckStateChanged != null)
                //        CheckStateChanged(this, e);
                //};
            }
        }

        public CheckBoxDropDown()
            : base()
        {
            popup = new ToolStripDropDownMenu();
            popup.ShowImageMargin = false;
            //popup.TopLevel = false;
            //popup.CanOverflow = true;
            popup.AutoClose = true;
            popup.DropShadowEnabled = true;

            InitializeComponent();
        }

        protected override void OnDropDown(EventArgs e)
        {
            base.OnDropDown(e);

            this.DropDownHeight = 1;

            if (popup.Visible)
                return;

            Rectangle rect = RectangleToScreen(this.ClientRectangle);
            Point location = new Point(rect.X, rect.Y + this.Size.Height);
            popup.Show(location, ToolStripDropDownDirection.BelowRight);
        }

        protected void Add(CheckBoxItem item)
        {
            items.Add(item);

            ToolStripCheckboxItem tsi = new ToolStripCheckboxItem(item);
            tsi.BackColor = this.BackColor;
            tsi.CheckedChanged += CheckedChanged;
            popup.Items.Add(tsi);
        }

        protected void Remove(CheckBoxItem item)
        {
            if (!items.Remove(item))
                return;

            // TODO: remove from popup
            //tsi.CheckedChanged -= tsi_CheckedChanged;
        }

        protected void Clear()
        {
            items.Clear();
            popup.Items.Clear();
        }

        private void CheckedChanged(object sender, EventArgs e)
        {
            ToolStripCheckboxItem item = sender as ToolStripCheckboxItem;
            if (item == null)
                return;

            UpdateText();
        }

        private void UpdateText()
        {
            List<string> selected = new List<string>();
            foreach (ToolStripItem item in popup.Items)
            {
                ToolStripCheckboxItem foo = item as ToolStripCheckboxItem;
                if (item == null)
                    continue;

                if (foo.Item.Checked)
                    selected.Add(foo.Item.Text);
            }

            string text = String.Join(",", selected.ToArray());
            SelectedText = text;
            //Text = text;
        }
    }
}
