using AsyncRAT_Sharp.MessagePack;
using AsyncRAT_Sharp.Sockets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

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
            if (listView1.SelectedItems.Count == 1)
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "fileManager";
                msgpack.ForcePathObject("Command").AsString = "getPath";
                msgpack.ForcePathObject("Path").AsString = listView1.SelectedItems[0].ToolTipText;
                C.BeginSend(msgpack.Encode2Bytes());
                toolStripStatusLabel1.Text = listView1.SelectedItems[0].ToolTipText;
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
                    C.BeginSend(msgpack.Encode2Bytes());
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
                C.BeginSend(msgpack.Encode2Bytes());
                return;
            }

        }
    }
}
