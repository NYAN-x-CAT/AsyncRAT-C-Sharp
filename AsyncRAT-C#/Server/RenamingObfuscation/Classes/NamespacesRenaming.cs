using Server.RenamingObfuscation.Interfaces;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.RenamingObfuscation.Classes
{
    public class NamespacesRenaming : IRenaming
    {
        private static Dictionary<string, string> _names = new Dictionary<string, string>();

        public ModuleDefMD Rename(ModuleDefMD module)
        {
            ModuleDefMD moduleToRename = module;
            moduleToRename.Name = Utils.GenerateRandomString();

            foreach (TypeDef type in moduleToRename.GetTypes())
            {
                if (type.IsGlobalModuleType)
                    continue;

                if (type.Namespace == "")
                    continue;

                string nameValue;
                if (_names.TryGetValue(type.Namespace, out nameValue))
                    type.Namespace = nameValue;
                else
                {
                    string newName = Utils.GenerateRandomString();

                    _names.Add(type.Namespace, newName);
                    type.Namespace = newName;
                }
            }

            return ApplyChangesToResources(moduleToRename);
        }

        private static ModuleDefMD ApplyChangesToResources(ModuleDefMD module)
        {
            ModuleDefMD moduleToRename = module;

            foreach (var resource in moduleToRename.Resources)
            {
                foreach (var item in _names)
                {
                    if (resource.Name.Contains(item.Key))
                    {
                        resource.Name = resource.Name.Replace(item.Key, item.Value);
                    }
                }
            }

            foreach (TypeDef type in moduleToRename.GetTypes())
            {
                foreach (var property in type.Properties)
                {
                    if (property.Name != "ResourceManager")
                        continue;

                    var instr = property.GetMethod.Body.Instructions;

                    for (int i = 0; i < instr.Count; i++)
                    {
                        if (instr[i].OpCode == OpCodes.Ldstr)
                        {
                            foreach (var item in _names)
                            {
                                if (instr[i].ToString().Contains(item.Key))
                                    instr[i].Operand = instr[i].Operand.ToString().Replace(item.Key, item.Value);
                            }
                        }
                    }
                }
            }

            return moduleToRename;
        }
    }
}
