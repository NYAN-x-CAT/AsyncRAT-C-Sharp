using dnlib.DotNet;

namespace Server.RenamingObfuscation.Interfaces
{
    public interface IRenaming
   {
        ModuleDefMD Rename(ModuleDefMD module);
    }
}
