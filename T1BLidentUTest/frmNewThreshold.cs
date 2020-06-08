using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace T1BLidentUTest
{
    public partial class frmNewThreshold : Form
    {
        public string THRESHOLD
        {
            get { return numericUpDown1.Value.ToString(); }
        }

        public frmNewThreshold(string val)
        {
            InitializeComponent();

            if (Int32.Parse(val) < 0) val = "0";

            numericUpDown1.Value = Int32.Parse(val);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
