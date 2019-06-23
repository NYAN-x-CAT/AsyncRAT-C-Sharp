using System;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using Server.Helper;
using System.Security.Cryptography.X509Certificates;

namespace Server.Forms
{
    public partial class FormPorts : Form
    {
        private static bool isOK = false;
        public FormPorts()
        {
            InitializeComponent();
            this.Opacity = 0;
        }

        private void PortsFrm_Load(object sender, EventArgs e)
        {
            _ = Methods.FadeIn(this, 5);

            if (Properties.Settings.Default.Ports.Length == 0)
            {
                listBox1.Items.AddRange(new object[] { "6606", "7707", "8808" });
            }
            else
            {
                try
                {
                    string[] ports = Properties.Settings.Default.Ports.Split(new[] { "," }, StringSplitOptions.None);
                    foreach (string item in ports)
                    {
                        if (!string.IsNullOrWhiteSpace(item))
                            listBox1.Items.Add(item.Trim());
                    }
                }
                catch { }
            }

            this.Text = $"{Settings.Version} | Welcome {Environment.UserName}";

            if (!File.Exists(Settings.CertificatePath))
            {
                using (FormCertificate formCertificate = new FormCertificate())
                {
                    formCertificate.ShowDialog();
                }
            }
            else
            {
                Settings.ServerCertificate = new X509Certificate2(Settings.CertificatePath);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count > 0)
            {
                string ports = "";
                foreach (string item in listBox1.Items)
                {
                    ports += item + ",";
                }
                Properties.Settings.Default.Ports = ports.Remove(ports.Length - 1);
                Properties.Settings.Default.Save();
                isOK = true;
                this.Close();
            }
        }

        private void PortsFrm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!isOK)
            {
                Program.form1.notifyIcon1.Dispose();
                Environment.Exit(0);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                Convert.ToInt32(textPorts.Text.Trim());
                listBox1.Items.Add(textPorts.Text.Trim());
                textPorts.Clear();
            }
            catch { }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            listBox1.Items.Remove(listBox1.SelectedItem);
        }
    }
}
