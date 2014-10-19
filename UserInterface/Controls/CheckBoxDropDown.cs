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
        public long Value;
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
            public long Value { get { return item.Value; } }
            public new string Text { get { return item.Text; } }
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
                checkbox.AutoSize = true;
                checkbox.Checked = item.Checked;
                checkbox.Text = item.Text;
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

            UpdateText();
        }

        protected void Remove(CheckBoxItem item)
        {
            if (!items.Remove(item))
                return;

            // TODO: remove from popup
            //tsi.CheckedChanged -= tsi_CheckedChanged;

            UpdateText();
        }

        protected void Update(CheckBoxItem item)
        {
            bool altered = false;
            foreach (CheckBoxItem foo in items)
            {
                if (item.Value != foo.Value)
                    continue;

                if (foo.Checked != item.Checked)
                {
                    foreach (ToolStripCheckboxItem bar in popup.Items)
                    {
                        if (foo != bar.Item)
                            continue;

                        bar.CheckBox.Checked = item.Checked;
                        altered = true;
                    }
                }

                //TODO: hotwo handle Text change?
            }

            if (altered)
                UpdateText();
        }

        protected void Clear()
        {
            items.Clear();
            popup.Items.Clear();

            UpdateText();
        }

        private void CheckedChanged(object sender, EventArgs e)
        {
            ToolStripCheckboxItem item = sender as ToolStripCheckboxItem;
            if (item == null)
                return;

            UpdateText();
            CheckedChanged(item.Item);
        }

        protected virtual void CheckedChanged(CheckBoxItem item)
        {
        }

        private void UpdateText()
        {
            List<CheckBoxItem> checkedItems = new List<CheckBoxItem>();
            foreach (ToolStripItem item in popup.Items)
            {
                ToolStripCheckboxItem foo = item as ToolStripCheckboxItem;
                if (item == null)
                    continue;

                if (foo.Item.Checked)
                    checkedItems.Add(foo.Item);
            }
            string text = GetPresentationText(checkedItems);

            // Set presentation text (simulate selected item)
            this.Items.Clear();
            this.Items.Add(text);
            this.SelectedIndex = 0;
        }

        private string GetPresentationText(IEnumerable<CheckBoxItem> checkedItems)
        {
            List<string> names = new List<string>();
            foreach (CheckBoxItem item in checkedItems)
                names.Add(item.Text);
            return String.Join(",", names.ToArray());
        }
    }
}
