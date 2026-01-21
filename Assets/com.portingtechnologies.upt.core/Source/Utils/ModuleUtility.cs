using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UPT.Core
{
    public static class ModuleUtility
    {
        public static List<Type> GetAllAvailableServiceTypes(IReadOnlyList<IPlatformModule> modules)
        {
            var serviceTypes = new List<Type>();

            foreach (var module in modules)
            {
                var providedServices = module.ProvidedServiceTypes;
                foreach (var serviceType in providedServices)
                {
                    if (!serviceTypes.Contains(serviceType))
                        serviceTypes.Add(serviceType);
                }
            }

            return serviceTypes;
        }

        public static bool IsSystemAssembly(Assembly assembly)
        {
            return Constants.SystemAssemblies.Any(name => assembly.FullName.StartsWith(name));
        }
    }
}
