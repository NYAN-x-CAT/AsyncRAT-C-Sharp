using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lunar.Kernal
{
    class SafeExit
    {
        [System.Runtime.InteropServices.DllImport("ntdll.dll")]
        public static extern uint RtlAdjustPrivilege(int Privilege, bool bEnablePrivilege, bool IsThreadPrivilege, out bool PreviousValue);

        [System.Runtime.InteropServices.DllImport("ntdll.dll")]
        public static extern uint NtRaiseHardError(uint ErrorStatus, uint NumberOfParameters, uint UnicodeStringParameterMask, IntPtr Parameters, uint ValidResponseOption, out uint Response);

        [System.Runtime.InteropServices.DllImport("ntdll.dll", SetLastError = true)]
        private static extern void RtlSetProcessIsCritical(UInt32 v1, UInt32 v2, UInt32 v3);

        public static void Exit()
        {
            Boolean t1;
            RtlAdjustPrivilege(19, true, false, out t1);
            RtlSetProcessIsCritical(0, 0, 0);
            Environment.Exit(0);
        }
    }
}
