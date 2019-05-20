using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lunar.Kernal
{
    class KernalHooks
    {
        [DllImport("ntdll.dll")]
        public static extern uint RtlAdjustPrivilege(int Privilege, bool bEnablePrivilege, bool IsThreadPrivilege, out bool PreviousValue);

        [DllImport("ntdll.dll")]
        public static extern uint NtRaiseHardError(uint ErrorStatus, uint NumberOfParameters, uint UnicodeStringParameterMask, IntPtr Parameters, uint ValidResponseOption, out uint Response);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern void RtlSetProcessIsCritical(UInt32 v1, UInt32 v2, UInt32 v3);

        public static void System()
        {
            Process.EnterDebugMode();
            Boolean t1;
            RtlAdjustPrivilege(31, true, false, out t1);
            RtlSetProcessIsCritical(1, 0, 0);

        }
        public static void User()
        {
            Process.EnterDebugMode();
            Boolean t1;
            RtlAdjustPrivilege(19, true, false, out t1);
            RtlSetProcessIsCritical(0, 0, 0);
        }
    }
}
