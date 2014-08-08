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
            organizer.sourcePath = sourcePath.Path;
            organizer.destinationPath = destinationPath.Path;
            organizer.Recursive = recursive.Checked;
            organizer.DuplicateMode = (DuplicateMode)duplicateMode.SelectedItem;
            organizer.CopyMode = (CopyMode)copyMode.SelectedItem;
            organizer.DestinationPatternImage = patternImage.Text;
            organizer.DestinationPatternVideo = patternVideo.Text;
            organizer.DestinationPatternAudio = patternAudio.Text;
            organizer.Localization = (CultureInfo)localization.SelectedItem;

            ProgressStarted();

            Thread thread = new Thread(ParseThread);
            thread.IsBackground = true;
            thread.Name = "Media parse thread";
            thread.Start(organizer);
        }

        private void ProgressStarted()
        {
            organize.Enabled = false;
        }

        private void ProgressEnded()
        {
            organize.Enabled = true;
        }

        #region Parse

        private void ParseThread(object arg)
        {
            MediaOrganizer organizer = arg as MediaOrganizer;
            try
            {
                organizer.Parse();
                ParseComplete(organizer);
            }
            catch (Exception ex)
            {
                throw; // TODO: implement handler
            }
        }

        private void ParseComplete(MediaOrganizer organizer)
        {
            if (InvokeRequired)
            {
                Action<MediaOrganizer> action = new Action<MediaOrganizer>(ParseComplete);
                BeginInvoke(action, organizer);
                return;
            }

            if (MessageBox.Show(organizer.copyItems.ToString(), "Copy these?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                ProgressEnded();
                return;
            }

            Thread thread = new Thread(OrganizeThread);
            thread.IsBackground = true;
            thread.Name = "Media organize thread";
            thread.Start(organizer);
        }

        private void ParseException(Exception ex)
        {
            if (InvokeRequired)
            {
                Action<Exception> action = new Action<Exception>(ParseException);
                BeginInvoke(action, ex);
                return;
            }

            MessageBox.Show(ex.Message, "Media parse failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            ProgressEnded();
        }

        #endregion

        #region Organize

        private void OrganizeThread(object arg)
        {
            MediaOrganizer organizer = arg as MediaOrganizer;
            try
            {
                organizer.Organize();
                OrganizeComplete(organizer);
            }
            catch (Exception ex)
            {
                OrganizeException(ex);
            }
        }

        private void OrganizeComplete(MediaOrganizer organizer)
        {
            if (InvokeRequired)
            {
                Action<MediaOrganizer> action = new Action<MediaOrganizer>(OrganizeComplete);
                BeginInvoke(action, organizer);
                return;
            }

            MessageBox.Show("Media organization completed successfully", "Media organization done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ProgressEnded();
        }

        private void OrganizeException(Exception ex)
        {
            if (InvokeRequired)
            {
                Action<Exception> action = new Action<Exception>(OrganizeException);
                BeginInvoke(action, ex);
                return;
            }

            MessageBox.Show(ex.Message, "Media organization failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            ProgressEnded();
        }

        #endregion
    }
}
