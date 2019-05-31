using AsyncRAT_Sharp.RenamingObfuscation.Interfaces;
using dnlib.DotNet;

namespace AsyncRAT_Sharp.RenamingObfuscation.Classes
{
    public class PropertiesRenaming : IRenaming
    {
        public ModuleDefMD Rename(ModuleDefMD module)
        {
            ModuleDefMD moduleToRename = module;

            foreach (TypeDef type in moduleToRename.GetTypes())
            {
                if (type.IsGlobalModuleType)
                    continue;

                foreach (var property in type.Properties)
                {
                    property.Name = Utils.GenerateRandomString();
                }
            }

            return moduleToRename;
        }
    }
}
