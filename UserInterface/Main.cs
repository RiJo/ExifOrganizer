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

            organizer.LoadConfig();
            organizer.OnProgress += ReportProgress;
        }

        #region Control events

        private void Main_Load(object sender, EventArgs e)
        {
            infoVersion.Text = String.Format("Version: {0}", new Version(Application.ProductVersion).Get());

            sourcePath.Text = organizer.sourcePath;
            destinationPath.Text = organizer.destinationPath;
            recursive.Checked = organizer.Recursive;

            // FileComparator enum
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

            verifyFiles.Checked = organizer.VerifyFiles;

            // CultureInfo localization
            foreach (CultureInfo culture in CultureInfo.GetCultures(CultureTypes.AllCultures))
                localization.Items.Add(culture);
            localization.SelectedItem = organizer.Locale;

            // Patterns
            patternImage.Text = organizer.DestinationPatternImage;
            patternAudio.Text = organizer.DestinationPatternAudio;
            patternVideo.Text = organizer.DestinationPatternVideo;
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!organizer.IsRunning || organizer.IsAborted)
                return;

            if (MessageBox.Show(this, "Organization currently in progress, it must be aborted before the application can be closed.", "Abort current progress?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }

            organizer.Abort();
        }

        private void organize_Click(object sender, EventArgs e)
        {
            if (organizer.IsRunning)
            {
                AbortProgress();
                return;
            }

            organizer.sourcePath = sourcePath.SelectedPath;
            organizer.destinationPath = destinationPath.SelectedPath;
            organizer.Recursive = recursive.Checked;
            organizer.FileComparator = (FileComparator)fileComparator.EnumValue;
            organizer.CopyPrecondition = (CopyPrecondition)copyPrecondition.SelectedItem;
            organizer.CopyMode = (CopyMode)copyMode.SelectedItem;
            organizer.VerifyFiles = verifyFiles.Checked;
            organizer.DestinationPatternImage = patternImage.Text;
            organizer.DestinationPatternVideo = patternVideo.Text;
            organizer.DestinationPatternAudio = patternAudio.Text;
            organizer.Locale = (CultureInfo)localization.SelectedItem;
            organizer.SaveConfig();

            ProgressStarted();

            Thread thread = new Thread(ParseThread);
            thread.IsBackground = true;
            thread.Name = "Media parse thread";
            thread.Start(organizer);
        }

        #endregion

        private void AbortProgress()
        {
            organizer.Abort();
        }

        private void ReportProgress(MediaOrganizer organizer, double value, string message)
        {
            if (InvokeRequired)
            {
                Action<MediaOrganizer, double, string> action = new Action<MediaOrganizer, double, string>(ReportProgress);
                BeginInvoke(action, organizer, value, message);
                return;
            }

            double percent = value * 100.0;
            if (String.IsNullOrEmpty(message))
                message = String.Format("{0}%", Math.Round(percent, 1));
            else
                message = String.Format("{0}% - {1}", Math.Round(percent, 1), message);

            progress.SetProgress(value, message);
        }

        private void ProgressStarted()
        {
            if (InvokeRequired)
            {
                Action action = new Action(ProgressStarted);
                BeginInvoke(action);
                return;
            }

            organize.Text = "Abort";
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

            organize.Text = "Organize";
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

            Exception innerException = ex.GetInnerMost();
            MessageBox.Show(innerException.Message, "Media parse failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            Exception innerException = ex.GetInnerMost();
            MessageBox.Show(innerException.Message, "Media organization failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            ProgressEnded();
        }

        #endregion
    }
}
