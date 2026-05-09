using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UPT.Core
{
    public static class ModuleUtility
    {
        public static List<Type> GetAllAvailableServiceTypes(IList<IPlatformModule> modules)
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

        /// <summary>
        /// The module is active if at least one service uses it as a backend.
        /// </summary>
        public static bool IsModuleActive(IPlatformModule module, PlatformServiceCollection forCollection, IList<IPlatformModule> availableModules)
        {
            var availableServiceTypes = ModuleUtility.GetAllAvailableServiceTypes(availableModules);

            foreach (var serviceType in availableServiceTypes)
            {
                var service = forCollection.GetServiceConfig(serviceType);
                var currentPlatform = service?.PreferredPlatform ?? string.Empty;
                if (currentPlatform == module.DisplayName)
                    return true;
            }

            return false;
        }
    }
}
