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
            public event Action<object, EventArgs> CheckStateChanged;

            CheckBox checkbox;

            public CheckBox CheckBox { get { return checkbox; } }
            public bool Checked { get { return checkbox.Checked; } }
            public CheckState CheckState { get { return checkbox.CheckState; } }

            // TODO
            //public ToolStripCheckboxItem(string text, CheckState state = CheckState.Unchecked)
            //{

            //}

            public ToolStripCheckboxItem(string text, bool check = false)
                : base(new CheckBox())
            {
                checkbox = Control as CheckBox;
                checkbox.Checked = check;
                checkbox.CheckState = check ? CheckState.Checked : CheckState.Unchecked;
                checkbox.Text = text;
                checkbox.AutoSize = true;
                checkbox.CheckedChanged += delegate(object sender, EventArgs e)
                {

                    if (CheckedChanged != null)
                        CheckedChanged(this, e);
                };
                checkbox.CheckStateChanged += delegate(object sender, EventArgs e)
                {
                    if (CheckStateChanged != null)
                        CheckStateChanged(this, e);
                };
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

            foreach (string foo in new string[] { "x", "foo", "bar", "this is a very long string..." })
            {
                ToolStripItem item = new ToolStripCheckboxItem(foo);
                popup.Items.Add(item);
            }

            InitializeComponent();
        }

        protected override void OnDropDown(EventArgs e)
        {
            base.OnDropDown(e);

            if (popup.Visible)
                return;

            Rectangle rect = RectangleToScreen(this.ClientRectangle);
            Point location = new Point(rect.X, rect.Y + this.Size.Height);
            popup.Show(location, ToolStripDropDownDirection.BelowRight);

            //if (checkBoxes.Visible)
            //    return;
            this.DropDownHeight = 1;
            //this.DroppedDown = false;

            //ToolStripDropDown foo = new ToolStripDropDown();
            //ToolStripDrop
            //foo.Items.Add("hejsan");
            //foo.Show(this.Location);

            //checkBoxes.Show();
            //Form parent = this.FindForm();
            //if (parent == null)
            //    return;

            //popup.Show(this.FindForm());
            //SetPosition(popup);
        }

        protected void Add(CheckBoxItem item)
        {
            items.Add(item);

            ToolStripItem tsi = new ToolStripCheckboxItem(item.Text, item.Checked);
            popup.Items.Add(tsi);
        }

        protected void Remove(CheckBoxItem item)
        {
            if (!items.Remove(item))
                return;

            // TODO: remove from popup
        }

        protected void Clear()
        {
            items.Clear();
            popup.Items.Clear();
        }

        //protected override void OnDropDownClosed(EventArgs e)
        //{
        //    base.OnDropDownClosed(e);

        //    if (!popup.Visible)
        //        return;
        //    //popup.Hide();
        //}

        //private void SetPosition(Form form)
        //{
        //    Rectangle rect = RectangleToScreen(this.ClientRectangle);
        //    //form.Location = new Point(0, 0);
        //    form.Location = new Point(rect.X, rect.Y + this.Size.Height);

        //    //control.Location = new Point(rect.X + 50, rect.Y + this.Size.Height);
        //    //int count = control.Items.Count;
        //    //if (count > this.MaxDropDownItems)
        //    //{
        //    //    count = this.MaxDropDownItems;
        //    //}
        //    //else if (count == 0)
        //    //{
        //    //    count = 1;
        //    //}
        //    //form.Size = new Size(this.Size.Width, this.DropDownHeight);
        //}

        //private void DoDropDown()
        //{
        //    if (!dropdown.Visible)
        //    {
        //        Rectangle rect = RectangleToScreen(this.ClientRectangle);
        //        dropdown.Location = new Point(rect.X, rect.Y + this.Size.Height);
        //        int count = dropdown.List.Items.Count;
        //        if (count > this.MaxDropDownItems)
        //        {
        //            count = this.MaxDropDownItems;
        //        }
        //        else if (count == 0)
        //        {
        //            count = 1;
        //        }
        //        dropdown.Size = new Size(this.Size.Width,
        //                       (dropdown.List.ItemHeight + 1) * count);
        //        dropdown.Show(this);
        //    }
        //}

        //protected override void OnKeyDown(KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Down)
        //    {
        //        OnDropDown(null);
        //    }
        //    // Make sure that certain keys or combinations are not blocked.
        //    e.Handled = !e.Alt && !(e.KeyCode == Keys.Tab) &&
        //        !((e.KeyCode == Keys.Left) || (e.KeyCode == Keys.Right) ||
        //    (e.KeyCode == Keys.Home) || (e.KeyCode == Keys.End));
        //    base.OnKeyDown(e);
        //}

        //protected override void OnKeyPress(KeyPressEventArgs e)
        //{
        //    e.Handled = true;
        //    base.OnKeyPress(e);
        //}

        //protected override void OnDeactivate(EventArgs e)
        //{
        //    base.OnDeactivate(e);
        //    CCBoxEventArgs ce = e as CCBoxEventArgs;
        //    if (ce != null)
        //    {
        //        CloseDropdown(ce.AssignValues);
        //    }
        //    else
        //    {

        //        CloseDropdown(true);
        //    }
        //}
    }
}
