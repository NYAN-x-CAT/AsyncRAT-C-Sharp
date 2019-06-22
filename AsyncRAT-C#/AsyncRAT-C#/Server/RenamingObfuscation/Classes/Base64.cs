using Server.RenamingObfuscation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.RenamingObfuscation.Classes
{
    public class Base64 : ICrypto
    {
        /// <summary>
        /// Method for encrypt string with Base64. 
        /// </summary>
        /// <param name="dataPlain">Input plain string</param>
        /// <returns>Encode string</returns>
        public string Encrypt(string dataPlain)
        {
            try
            {
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(dataPlain));
            }

            catch (Exception)
            {
                return null;
            }
        }
    }
}
