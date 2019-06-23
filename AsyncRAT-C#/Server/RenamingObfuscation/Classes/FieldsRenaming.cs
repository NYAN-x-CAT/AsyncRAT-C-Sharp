using Server.RenamingObfuscation.Interfaces;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Collections.Generic;


namespace Server.RenamingObfuscation.Classes
{
    public class FieldsRenaming : IRenaming
    {
        private static Dictionary<string, string> _names = new Dictionary<string, string>();

        public ModuleDefMD Rename(ModuleDefMD module)
        {
            ModuleDefMD moduleToRename = module;

            foreach (TypeDef type in moduleToRename.GetTypes())
            {
                if (type.IsGlobalModuleType)
                    continue;

                foreach (var field in type.Fields)
                {
                    string nameValue;
                    if (_names.TryGetValue(field.Name, out nameValue))
                        field.Name = nameValue;
                    else
                    {
                        if (!field.IsSpecialName && !field.HasCustomAttributes)
                        {
                            string newName = Utils.GenerateRandomString();
                            _names.Add(field.Name, newName);
                            field.Name = newName;
                        }
                    }
                }
            }

            return ApplyChangesToResources(moduleToRename);
        }

        private static ModuleDefMD ApplyChangesToResources(ModuleDefMD module)
        {
            ModuleDefMD moduleToRename = module;

            foreach (TypeDef type in moduleToRename.GetTypes())
            {
                if (type.IsGlobalModuleType)
                    continue;

                foreach (MethodDef method in type.Methods)
                {
                    if (method.Name != "InitializeComponent")
                        continue;

                    var instr = method.Body.Instructions;

                    for (int i = 0; i < instr.Count - 3; i++)
                    {
                        if (instr[i].OpCode == OpCodes.Ldstr)
                        {
                            foreach (var item in _names)
                            {
                                if (item.Key == instr[i].Operand.ToString())
                                {
                                    instr[i].Operand = item.Value;
                                }
                            }
                        }
                    }
                }
            }

            return moduleToRename;
        }
    }
}
