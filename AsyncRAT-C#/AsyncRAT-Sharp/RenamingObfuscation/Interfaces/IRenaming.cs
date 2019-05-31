using dnlib.DotNet;

namespace AsyncRAT_Sharp.RenamingObfuscation.Interfaces
{
    public interface IRenaming
   {
        ModuleDefMD Rename(ModuleDefMD module);
    }
}
