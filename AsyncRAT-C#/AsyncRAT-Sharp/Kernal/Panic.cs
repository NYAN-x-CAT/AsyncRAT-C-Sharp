using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Lunar.Kernal
{
    class Panic
    {
        [DllImport("ntdll.dll")]
        public static extern uint RtlAdjustPrivilege(int Privilege, bool bEnablePrivilege, bool IsThreadPrivilege, out bool PreviousValue);

        [DllImport("ntdll.dll")]
        public static extern uint NtRaiseHardError(uint ErrorStatus, uint NumberOfParameters, uint UnicodeStringParameterMask, IntPtr Parameters, uint ValidResponseOption, out uint Response);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern void RtlSetProcessIsCritical(UInt32 v1, UInt32 v2, UInt32 v3);

        public static void RaiseHardError(uint ErrorCode)
        {
            Boolean e1;
            RtlAdjustPrivilege(19, true, false, out e1);
            uint e2;
            NtRaiseHardError(ErrorCode, 0, 0, IntPtr.Zero, 6, out e2); //Calls a BSOD with a custom error text
        }
    }
}
