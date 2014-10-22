﻿//
// FileBrowseControl.cs: User control for browsing files or directories.
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

namespace ExifOrganizer.UI
{
    public enum FileBrowseType
    {
        File,
        Directory
    }

    public partial class FileBrowseControl : UserControl
    {
        public FileBrowseControl()
            : base()
        {
            InitializeComponent();
        }

        public FileBrowseType BrowseType
        {
            get;
            set;
        }

        public bool ReadOnly
        {
            get { return path.ReadOnly; }
            set { path.ReadOnly = value; }
        }

        public override string Text
        {
            get { return path.Text; }
            set { path.Text = value; }
        }

        public string Path
        {
            get { return path.Text; }
            set { path.Text = value; }
        }

        public string DialogTitle
        {
            get;
            set;
        }

        private void browse_Click(object sender, EventArgs e)
        {
            if (BrowseType == FileBrowseType.File)
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Title = DialogTitle;
                dialog.Multiselect = false;
                dialog.FileName = path.Text;
                if (dialog.ShowDialog() == DialogResult.OK)
                    path.Text = dialog.FileName;
            }
            else
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog();
                dialog.Description = DialogTitle;
                dialog.SelectedPath = path.Text;
                if (dialog.ShowDialog() == DialogResult.OK)
                    path.Text = dialog.SelectedPath;
            }
        }
    }
}