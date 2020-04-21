using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server.Forms
{
    public partial class FormFileSearcher : Form
    {
        public FormFileSearcher()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtExtnsions.Text) && numericUpDown1.Value > 0)
            {
                this.DialogResult = DialogResult.OK;
            }
        }
    }
}
