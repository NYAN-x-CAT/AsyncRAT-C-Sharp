using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Server.Helper
{
    public class ReferenceLoader : MarshalByRefObject
    {
        public string[] LoadReferences(string assemblyPath)
        {
            try
            {
                var assembly = Assembly.ReflectionOnlyLoadFrom(assemblyPath);
                var paths = assembly.GetReferencedAssemblies().Select(x => x.FullName).ToArray();
                return paths;
            }
            catch { return null; }
        }

        public void AppDomainSetup(string assemblyPath)
        {
            try
            {
                var settings = new AppDomainSetup
                {
                    ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
                };
                var childDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString(), null, settings);

                var handle = Activator.CreateInstance(childDomain,
                           typeof(ReferenceLoader).Assembly.FullName,
                           typeof(ReferenceLoader).FullName,
                           false, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, null, CultureInfo.CurrentCulture, new object[0]);

                var loader = (ReferenceLoader)handle.Unwrap();
                //This operation is executed in the new AppDomain
                var paths = loader.LoadReferences(assemblyPath);
                AppDomain.Unload(childDomain);
                return;
            }
            catch { }
        }
    }
}
