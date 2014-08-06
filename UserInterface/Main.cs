//
// Main.cs: Graphical user interface (GUI) main window.
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
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExifOrganizer.Organizer;
using System.Reflection;
using System.Globalization;
using System.Threading;

namespace ExifOrganizer.UI
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            // DuplicateMode enum
            foreach (DuplicateMode mode in Enum.GetValues(typeof(DuplicateMode)))
                duplicateMode.Items.Add(mode);
            duplicateMode.SelectedIndex = 0;

            // CopyMode enum
            foreach (CopyMode mode in Enum.GetValues(typeof(CopyMode)))
                copyMode.Items.Add(mode);
            copyMode.SelectedIndex = 0;

            // CultureInfo localization
            CultureInfo culture = Thread.CurrentThread.CurrentUICulture;
            localization.Items.Add(culture);
            localization.SelectedItem = culture;
        }

        private void organize_Click(object sender, EventArgs e)
        {
            MediaOrganizer organizer = new MediaOrganizer();
            organizer.DuplicateMode = (DuplicateMode)duplicateMode.SelectedItem;
            organizer.CopyMode = (CopyMode)copyMode.SelectedItem;
            organizer.DestinationPatternImage = patternImage.Text;
            organizer.DestinationPatternVideo = patternVideo.Text;
            organizer.DestinationPatternAudio = patternAudio.Text;
            organizer.Localization = (CultureInfo)localization.SelectedItem;
            organizer.Recursive = recursive.Checked;

            CopyItems items = organizer.Parse(sourcePath.Path, destinationPath.Path);
            if (MessageBox.Show(items.ToString(), "Copy these?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            try
            {
                organizer.Organize(items);
                MessageBox.Show("Media organization completed successfully", "Media organization done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Failed to organize media", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }




    }
}
