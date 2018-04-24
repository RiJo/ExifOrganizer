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
using System.Drawing;
using System.Linq;
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
        private readonly string TEXT_ITEM_ALL = "<All>";
        private readonly string TEXT_ITEM_NONE = "<None>";

        private ToolStripCheckboxItem checkboxAll;
        private ToolStripCheckboxItem checkboxNone;
        private ToolStripDropDownMenu popup;

        protected class ToolStripCheckboxItem : ToolStripControlHost
        {
            public event Action<object, EventArgs> CheckedChanged;

            public event Action<object, EventArgs> CheckStateChanged;

            private CheckBoxItem item;
            private CheckBox checkbox;

            public CheckBoxItem Item { get { return item; } }
            public CheckBox CheckBox { get { return checkbox; } }
            public long Value { get { return item.Value; } }
            public new string Text { get { return checkbox.Text; } }
            public bool Checked { get { return checkbox.Checked; } }
            public CheckState CheckState { get { return checkbox.CheckState; } }

            public ToolStripCheckboxItem(CheckBoxItem source, bool allowUserCheck = true, bool allowUserUncheck = true)
                : base(new CheckBox())
            {
                item = source;
                checkbox = Control as CheckBox;
                checkbox.Checked = item.Checked;
                checkbox.Text = item.Text;
                checkbox.AutoCheck = false;
                checkbox.Click += delegate (object sender, EventArgs e)
                {
                    if (checkbox.Checked && !allowUserUncheck)
                        return;
                    if (!checkbox.Checked && !allowUserCheck)
                        return;
                    checkbox.Checked = !checkbox.Checked;
                };
                checkbox.CheckedChanged += delegate (object sender, EventArgs e)
                {
                    item.Checked = checkbox.Checked;

                    if (CheckedChanged != null)
                        CheckedChanged(this, e);
                };
                checkbox.CheckStateChanged += delegate (object sender, EventArgs e)
                {
                    if (CheckStateChanged != null)
                        CheckStateChanged(this, e);
                };
            }

            public void SetWidth(int width)
            {
                Width = width;
                checkbox.Width = Width;
                checkbox.MinimumSize = new Size(width - 35, checkbox.MinimumSize.Height);
            }

            public void SetText(string text)
            {
                if (text == Text)
                    return;

                item.Text = text;
                checkbox.Text = text;
            }

            public void SetChecked(bool value)
            {
                if (value == Checked)
                    return;

                item.Checked = value;
                checkbox.Checked = value;
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

        public bool CheckBoxNone
        {
            get { return checkboxNone != null; }
            set
            {
                if (value == CheckBoxNone)
                    return;

                if (value)
                {
                    bool check = GetCheckBoxItems().All(x => !x.Checked);

                    checkboxNone = new ToolStripCheckboxItem(new CheckBoxItem() { Value = 0, Text = TEXT_ITEM_NONE, Checked = check }, true, false);
                    checkboxNone.BackColor = this.BackColor;
                    checkboxNone.CheckedChanged += CheckedChangedNone;
                    popup.Items.Add(checkboxNone);
                }
                else
                {
                    popup.Items.Remove(checkboxNone);
                    checkboxNone.CheckedChanged -= CheckedChangedNone;
                    checkboxNone = null;
                }

                ItemsAltered();
            }
        }

        public bool CheckBoxAll
        {
            get { return checkboxAll != null; }
            set
            {
                if (value == CheckBoxAll)
                    return;

                if (value)
                {
                    bool check = GetCheckBoxItems().All(x => x.Checked);

                    checkboxAll = new ToolStripCheckboxItem(new CheckBoxItem() { Value = Int64.MaxValue, Text = TEXT_ITEM_ALL, Checked = check }, true, false);
                    checkboxAll.BackColor = this.BackColor;
                    checkboxAll.CheckedChanged += CheckedChangedAll;
                    popup.Items.Add(checkboxAll);
                }
                else
                {
                    popup.Items.Remove(checkboxAll);
                    checkboxAll.CheckedChanged -= CheckedChangedAll;
                    checkboxAll = null;
                }

                ItemsAltered();
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            foreach (ToolStripCheckboxItem tsi in GetToolStripCheckboxItems())
                tsi.SetWidth(ClientSize.Width);
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

        protected ToolStripCheckboxItem Add(CheckBoxItem item)
        {
            ToolStripCheckboxItem tsi = new ToolStripCheckboxItem(item);
            tsi.BackColor = this.BackColor;
            tsi.CheckedChanged += CheckedChanged;
            popup.Items.Add(tsi);

            ItemsAltered();

            return tsi;
        }

        protected void Remove(CheckBoxItem item)
        {
            int altered = 0;
            foreach (ToolStripCheckboxItem tsi in GetToolStripCheckboxItems())
            {
                if (tsi.Value == item.Value)
                {
                    tsi.CheckedChanged -= CheckedChanged;
                    popup.Items.Remove(tsi);
                    altered++;
                }
            }

            if (altered > 0)
                ItemsAltered();
        }

        protected void Update(CheckBoxItem item)
        {
            foreach (ToolStripCheckboxItem tsi in GetToolStripCheckboxItems())
            {
                if (item.Value != tsi.Item.Value)
                    continue;

                if (item.Text != tsi.Item.Text)
                    tsi.SetText(item.Text);

                if (item.Checked != tsi.Item.Checked)
                    tsi.SetChecked(item.Checked);
            }
        }

        protected void Clear()
        {
            if (popup.Items.Count == 0)
                return;

            bool none = CheckBoxNone;
            bool all = CheckBoxAll;

            CheckBoxNone = false;
            CheckBoxAll = false;

            popup.Items.Clear();

            CheckBoxNone = none;
            CheckBoxAll = all;

            ItemsAltered();
        }

        private void CheckedChanged(object sender, EventArgs e)
        {
            ToolStripCheckboxItem tsi = sender as ToolStripCheckboxItem;
            if (tsi == null)
                return;

            UpdateText();
            ItemsAltered();
            CheckedChanged(tsi.Item);
        }

        private void CheckedChangedNone(object sender, EventArgs e)
        {
            ToolStripCheckboxItem tsi = sender as ToolStripCheckboxItem;
            if (tsi == null)
                return;
            if (!tsi.Checked)
                return;

            if (checkboxAll != null)
                checkboxAll.SetChecked(false);

            foreach (ToolStripCheckboxItem item in GetToolStripCheckboxItems())
            {
                if (!item.Checked)
                    continue;
                item.SetChecked(false);
            }

            UpdateText();
        }

        private void CheckedChangedAll(object sender, EventArgs e)
        {
            ToolStripCheckboxItem tsi = sender as ToolStripCheckboxItem;
            if (tsi == null)
                return;
            if (!tsi.Checked)
                return;
            if (checkboxNone != null)
                checkboxNone.SetChecked(false);

            foreach (ToolStripCheckboxItem item in GetToolStripCheckboxItems())
            {
                if (item.Checked)
                    continue;
                item.SetChecked(true);
            }

            UpdateText();
        }

        protected virtual void CheckedChanged(CheckBoxItem item)
        {
        }

        private void ItemsAltered()
        {
            IEnumerable<CheckBoxItem> items = GetCheckBoxItems();
            bool all = items.Any();
            bool none = true;
            foreach (CheckBoxItem item in items)
            {
                all &= item.Checked;
                none &= !item.Checked;
            }

            if (checkboxNone != null && checkboxNone.Checked != none)
                checkboxNone.SetChecked(none);
            if (checkboxAll != null && checkboxAll.Checked != all)
                checkboxAll.SetChecked(all);

            UpdateText();
        }

        private void UpdateText()
        {
            int itemCount = 0;
            List<CheckBoxItem> checkedItems = new List<CheckBoxItem>();
            foreach (CheckBoxItem item in GetCheckBoxItems())
            {
                itemCount++;
                if (item.Checked)
                    checkedItems.Add(item);
            }

            string text = GetPresentationText(checkedItems, itemCount);

            // Set presentation text (simulate selected item)
            this.Items.Clear();
            this.Items.Add(text);
            this.SelectedIndex = 0;
        }

        private IEnumerable<CheckBoxItem> GetCheckBoxItems()
        {
            return GetToolStripCheckboxItems().Select(x => x.Item);
        }

        private IEnumerable<ToolStripCheckboxItem> GetToolStripCheckboxItems()
        {
            foreach (ToolStripItem temp in popup.Items)
            {
                ToolStripCheckboxItem tsi = temp as ToolStripCheckboxItem;
                if (tsi == null)
                    continue;

                if (tsi.Value == 0 || tsi.Value == Int64.MaxValue)
                    continue;

                yield return tsi;
            }
        }

        private string GetPresentationText(IEnumerable<CheckBoxItem> checkedItems, int totalItemCount)
        {
            int checkedItemCount = checkedItems.Count();
            if (checkedItemCount == 0)
                return TEXT_ITEM_NONE;
            if (checkedItemCount == totalItemCount)
                return TEXT_ITEM_ALL;

            return String.Join(",", checkedItems.Select(x => x.Text));
        }
    }
}