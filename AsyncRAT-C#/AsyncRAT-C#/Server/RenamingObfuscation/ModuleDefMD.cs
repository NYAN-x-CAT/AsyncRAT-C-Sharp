using Server.RenamingObfuscation.Classes;
using Server.RenamingObfuscation.Interfaces;
using dnlib.DotNet;

// Credit github.com/srn-g/RenamingObfuscation
// Fxied by nyan cat
namespace Server.RenamingObfuscation
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
