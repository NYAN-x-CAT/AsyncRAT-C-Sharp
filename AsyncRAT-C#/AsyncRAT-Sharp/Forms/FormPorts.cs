using System;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using AsyncRAT_Sharp.Helper;
using System.Security.Cryptography.X509Certificates;

namespace AsyncRAT_Sharp.Forms
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
            Methods.FadeIn(this, 5);

            textPorts.Text = "6606, 7707, 8808";
            if (Properties.Settings.Default.Ports.Length > 0)
                textPorts.Text = Properties.Settings.Default.Ports;

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
            if (textPorts.Text.Length > 0)
            {
                Properties.Settings.Default.Ports = textPorts.Text;
                Properties.Settings.Default.Save();
                isOK = true;
                this.Close();
            }
        }

        private void PortsFrm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!isOK)
                Environment.Exit(0);
        }
    }
}
