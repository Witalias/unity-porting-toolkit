using System;
using System.Collections.Generic;
using System.Linq;
using UPT.Core;

namespace UPT.Editor
{
    public static class ModuleDiscovery
    {
        public static List<IPlatformModule> FindAvailableModules()
        {
            var modules = new List<IPlatformModule>();

            // 1. Look for the core.
            var coreAssembly = typeof(IPlatformModule).Assembly;

            // 2. Look at ALL the assemblies in the project.
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in allAssemblies)
            {
                // 3. Look for those that link to our core.
                var referencedAssemblies = assembly.GetReferencedAssemblies();
                if (referencedAssemblies.Any(assembly => assembly.Name == coreAssembly.GetName().Name))
                {
                    // 4. Look for module classes in these assemblies.
                    var moduleTypes = assembly.GetTypes()
                        .Where(type => typeof(IPlatformModule).IsAssignableFrom(type));

                    foreach (var type in moduleTypes)
                    {
                        // 5. Create module instances.
                        var module = Activator.CreateInstance(type) as IPlatformModule;
                        modules.Add(module);
                    }
                }
            }

            return modules;
        }
    }
}
