using System;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
namespace AsyncRAT_Sharp.Forms
{
    public partial class PortsFrm : Form
    {
        private static bool isOK = false;
        public PortsFrm()
        {
            InitializeComponent();
            this.Opacity = 0;
        }

        private void PortsFrm_Load(object sender, EventArgs e)
        {
            Methods.FadeIn(this, 5);

            textPorts.Text = Settings.Port;

            if (Properties.Settings.Default.Ports.Length > 0)
                textPorts.Text = Properties.Settings.Default.Ports;

            if (Properties.Settings.Default.Password.Length > 0)
                textPassword.Text = Properties.Settings.Default.Password;

            this.Text = $"{Settings.Version} | Welcome {Environment.UserName}";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textPorts.Text.Length > 0 && textPassword.Text.Length > 0)
            {
                Properties.Settings.Default.Ports = textPorts.Text;
                Properties.Settings.Default.Password = textPassword.Text;
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
