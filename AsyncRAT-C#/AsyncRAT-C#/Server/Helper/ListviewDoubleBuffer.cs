using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server.Helper
{
    public static class ListviewDoubleBuffer
    {
        public static void Enable(ListView listView)
        {
            PropertyInfo aProp = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance);
            aProp.SetValue(listView, true, null);
        }
    }
}