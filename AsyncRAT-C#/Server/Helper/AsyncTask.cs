using Server.MessagePack;
using Server.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace Server.Helper
{
    public class AsyncTask
    {
        public byte[] msgPack;
        public string id;
        public List<string> doneClient;

        public AsyncTask(byte[] _msgPack, string _id)
        {
            msgPack = _msgPack;
            id = _id;
            doneClient = new List<string>();
        }
    }

}