using Server.RenamingObfuscation.Interfaces;
using dnlib.DotNet;


namespace Server.RenamingObfuscation.Classes
{
    public class MethodsRenaming : IRenaming
    {
        public ModuleDefMD Rename(ModuleDefMD module)
        {
            ModuleDefMD moduleToRename = module;

            foreach (TypeDef type in moduleToRename.Types)
            {
                if (type.IsGlobalModuleType)
                    continue;
                 type.Name = Utils.GenerateRandomString();
                foreach (MethodDef method in type.Methods)
                {
                    if (!method.IsSpecialName && !method.IsConstructor && !method.HasCustomAttributes && !method.IsAbstract && !method.IsVirtual && method.Name != "Main")
                        method.Name = Utils.GenerateRandomString();

                    foreach (ParamDef paramDef in method.ParamDefs)
                    {
                        paramDef.Name = Utils.GenerateRandomString();
                    }
                }
            }

            return moduleToRename;
        }
    }
}