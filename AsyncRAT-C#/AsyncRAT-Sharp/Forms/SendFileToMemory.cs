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

namespace AsyncRAT_Sharp
{
    public partial class SendFileToMemory : Form
    {
        public SendFileToMemory()
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
                if (comboBox1.SelectedIndex == 0)
                {
                    try
                    {
                        Assembly.LoadFile(O.FileName);
                    }
                    catch
                    {
                        toolStripStatusLabel1.ForeColor = Color.Red;
                    }
                }
            }
            else
            {
                toolStripStatusLabel1.Text = "";
                toolStripStatusLabel1.ForeColor = Color.Black;
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (toolStripStatusLabel1.ForeColor == Color.Green && toolStripStatusLabel1.Text.Length > 0) this.Hide();
        }
    }
}
