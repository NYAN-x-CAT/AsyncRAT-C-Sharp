using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Client.Helper
{
   public static class MutexControl
    {
        public static Mutex currentApp;
        public static bool CreateMutex()
        {
            currentApp = new Mutex(false, Settings.MTX, out bool createdNew);
            return createdNew;
        }
        public static void CloseMutex()
        {
            if (currentApp != null)
            {
                currentApp.Close();
                currentApp = null;
            }
        }
    }
}
