using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MediaOrganizer;

namespace UserInterface
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {

        }

        private void parse_Click(object sender, EventArgs e)
        {
            Organizer organizer = new Organizer();
            CopyItems items = organizer.Parse(@"C:\temp", @"C:\temp\backup");
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
