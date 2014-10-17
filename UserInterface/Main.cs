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
        MediaOrganizer organizer = new MediaOrganizer();

        public Main()
        {
            InitializeComponent();

            organizer.OnProgress += ReportProgress;
        }

        #region Control events

        private void Main_Load(object sender, EventArgs e)
        {
            // DuplicateMode enum
            //foreach (FileComparator comparator in Enum.GetValues(typeof(FileComparator)))
            //    fileComparator.Items.Add(comparator);
            //fileComparator.Items.Add(organizer.FileComparator);
            //fileComparator.SelectedItem = organizer.FileComparator;
            fileComparator.EnumType = typeof(FileComparator);
            fileComparator.EnumValue = organizer.FileComparator;

            // CopyPrecondition enum
            foreach (CopyPrecondition precondition in Enum.GetValues(typeof(CopyPrecondition)))
                copyPrecondition.Items.Add(precondition);
            copyPrecondition.SelectedItem = organizer.CopyPrecondition;

            // CopyMode enum
            foreach (CopyMode mode in Enum.GetValues(typeof(CopyMode)))
                copyMode.Items.Add(mode);
            copyMode.SelectedItem = organizer.CopyMode;

            // CultureInfo localization
            localization.Items.Add(organizer.Localization);
            localization.SelectedItem = organizer.Localization;

            // Patterns
            patternImage.Text = organizer.DestinationPatternImage;
            patternAudio.Text = organizer.DestinationPatternAudio;
            patternVideo.Text = organizer.DestinationPatternVideo;
        }

        private void organize_Click(object sender, EventArgs e)
        {
            organizer.sourcePath = sourcePath.Path;
            organizer.destinationPath = destinationPath.Path;
            organizer.Recursive = recursive.Checked;
            organizer.FileComparator = (FileComparator)fileComparator.EnumValue;
            organizer.CopyPrecondition = (CopyPrecondition)copyPrecondition.SelectedItem;
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

        #endregion

        private void ReportProgress(MediaOrganizer organizer, double value)
        {
            if (InvokeRequired)
            {
                Action<MediaOrganizer, double> action = new Action<MediaOrganizer, double>(ReportProgress);
                BeginInvoke(action, organizer, value);
                return;
            }

            int interval = (progress.Maximum - progress.Minimum);
            int current = progress.Minimum + (int)Math.Round(value * interval);
            if (current > progress.Maximum)
                progress.Value = progress.Maximum;
            else if (current < progress.Minimum)
                progress.Value = progress.Minimum;
            else
                progress.Value = current;
        }

        private void ProgressStarted()
        {
            if (InvokeRequired)
            {
                Action action = new Action(ProgressStarted);
                BeginInvoke(action);
                return;
            }

            organize.Enabled = false;
            progress.Value = progress.Minimum;
            progress.Visible = true;
        }

        private void ProgressEnded()
        {
            if (InvokeRequired)
            {
                Action action = new Action(ProgressEnded);
                BeginInvoke(action);
                return;
            }

            organize.Enabled = true;
            progress.Visible = false;
        }

        #region Parse

        private void ParseThread(object arg)
        {
            MediaOrganizer organizer = arg as MediaOrganizer;
            try
            {
                OrganizeSummary summary = organizer.Parse();
                ParseComplete(organizer, summary);
            }
            catch (Exception ex)
            {
                ParseException(ex);
            }
        }

        private void ParseComplete(MediaOrganizer organizer, OrganizeSummary summary)
        {
            if (InvokeRequired)
            {
                Action<MediaOrganizer, OrganizeSummary> action = new Action<MediaOrganizer, OrganizeSummary>(ParseComplete);
                BeginInvoke(action, organizer, summary);
                return;
            }

            if (MessageBox.Show(summary.ToString(), "Continue organization?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
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
