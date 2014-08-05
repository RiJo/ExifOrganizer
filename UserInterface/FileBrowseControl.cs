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
