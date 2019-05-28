using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

//       │ Author     : NYAN CAT
//       │ Name       : AsyncRAT // Simple RAT

//       Contact Me   : https://github.com/NYAN-x-CAT

//       This program Is distributed for educational purposes only.

//       Credits;
//       Serialization    @ymofen
//       StreamLibrary    @Dergan
//       Special Thanks   MaxXor@hf gigajew@hf

namespace AsyncRAT_Sharp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Process Self = Process.GetCurrentProcess();
            Self.PriorityClass = ProcessPriorityClass.RealTime; //Warning! Dont RealTime & Marshel

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            form1 = new Form1();
            Application.Run(form1);
        }
        public static Form1 form1;
    }
}
