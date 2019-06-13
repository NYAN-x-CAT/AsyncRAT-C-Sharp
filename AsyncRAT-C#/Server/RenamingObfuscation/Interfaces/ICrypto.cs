using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.RenamingObfuscation.Interfaces
{
    public interface ICrypto
    {
        string Encrypt(string dataPlain);
    }
}
