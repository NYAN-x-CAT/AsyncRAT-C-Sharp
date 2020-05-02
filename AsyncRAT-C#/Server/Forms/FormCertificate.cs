using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.IO.Compression;

namespace Server.Forms
{
    public partial class FormCertificate : Form
    {
        public FormCertificate()
        {
            InitializeComponent();
        }

        private void FormCertificate_Load(object sender, EventArgs e)
        {
            try
            {
                string backup = Application.StartupPath + "\\BackupCertificate.zip";
                if (File.Exists(backup))
                {
                    MessageBox.Show(this, "Found a zip backup, Extracting (BackupCertificate.zip)", "Certificate backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ZipFile.ExtractToDirectory(backup, Application.StartupPath);
                    Settings.ServerCertificate = new X509Certificate2(Settings.CertificatePath);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Certificate", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private async void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(textBox1.Text)) return;

                button1.Text = "Please wait";
                button1.Enabled = false;
                textBox1.Enabled = false;
                await Task.Run(() =>
                {
                    try
                    {
                        string backup = Application.StartupPath + "\\BackupCertificate.zip";
                        Settings.ServerCertificate = Helper.CreateCertificate.CreateCertificateAuthority(textBox1.Text, 4096);
                        File.WriteAllBytes(Settings.CertificatePath, Settings.ServerCertificate.Export(X509ContentType.Pkcs12));

                        using (ZipArchive archive = ZipFile.Open(backup, ZipArchiveMode.Create))
                        {
                            archive.CreateEntryFromFile(Settings.CertificatePath, Path.GetFileName(Settings.CertificatePath));
                        }
                        Program.form1.listView1.BeginInvoke((MethodInvoker)(() =>
                        {
                            MessageBox.Show(this, @"
[!] If you wish to upgrade to new version of AsyncRAT, You will need to copy 'ServerCertificate.p12'.

[!] If you lose\delete 'ServerCertificate.p12' certificate you will NOT be able to control your clients, You will lose them all.", "Certificate", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.Close();
                        }));
                    }
                    catch (Exception ex)
                    {
                        Program.form1.listView1.BeginInvoke((MethodInvoker)(() =>
                        {
                            MessageBox.Show(this, ex.Message, "Certificate", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            button1.Text = "OK";
                            button1.Enabled = true;
                            textBox1.Enabled = true;
                        }));
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Certificate", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                button1.Text = "Ok";
                button1.Enabled = true;
            }
        }

    }
}
