using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

/* 
       │ Author       : NYAN CAT
       │ Name         : AsyncRAT  Simple RAT
       │ Contact Me   : https:github.com/NYAN-x-CAT

       This program Is distributed for educational purposes only.
*/

namespace Server
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                string batPath = Path.Combine(Application.StartupPath, "Fixer.bat");
                if (!File.Exists(batPath))
                    File.WriteAllText(batPath, Properties.Resources.Fixer);
            }
            catch { }
            form1 = new Form1();
            Application.Run(form1);
        }
        public static Form1 form1;
    }
}
