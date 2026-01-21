using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace UPT.Core
{
    public class PlatformModuleManager
    {
        private readonly List<IPlatformModule> m_loadedModules = new();

        public IReadOnlyList<IPlatformModule> LoadedModules => m_loadedModules;

        public void LoadModules()
        {
            UptLogger.Info("Starting module discovery...");
            var discoveredModules = DiscoverModulesViaReflection();
            InitializeAndRegisterModules(discoveredModules);
            FillMockServices();
            LogLoadingSummary();
        }

        public void UnloadAllModules()
        {
            foreach (var module in m_loadedModules)
            {
                try
                {
                    module.Shutdown();
                }
                catch (Exception e)
                {
                    UptLogger.Error($"Failed to unload module {GetModuleName(module)}: {e.Message}");
                }
            }
            m_loadedModules.Clear();
            UptLogger.Info($"Unloading modules completed");
        }

        private IList<IPlatformModule> DiscoverModulesViaReflection()
        {
            var modules = new List<IPlatformModule>();

            try
            {
                var coreAssembly = typeof(IPlatformModule).Assembly;
                var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

                UptLogger.Info($"Scanning {allAssemblies.Length} assemblies for modules...");

                foreach (var assembly in allAssemblies)
                {
                    if (ModuleUtility.IsSystemAssembly(assembly) || assembly == coreAssembly)
                        continue;

                    try
                    {
                        if (assembly.GetReferencedAssemblies()
                            .Any(referenced => referenced.Name == coreAssembly.GetName().Name))
                        {
                            var assemblyModules = FindModulesInAssembly(assembly);
                            modules.AddRange(assemblyModules);
                        }
                    }
                    catch (Exception e)
                    {
                        UptLogger.Warning($"Failed to process assembly '{assembly.GetName().Name}': {e.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                UptLogger.Error($"Module discovery failed: {e.Message}");
            }

            return modules;
        }

        private IList<IPlatformModule> FindModulesInAssembly(Assembly assembly)
        {
            var modules = new List<IPlatformModule>();

            try
            {
                var moduleTypes = assembly.GetTypes()
                    .Where(type => typeof(IPlatformModule).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                    .ToList();

                foreach (var type in moduleTypes)
                {
                    try
                    {
                        if (Activator.CreateInstance(type) is IPlatformModule module)
                        {
                            //UptLogger.Info($"Created module instance: {module.DisplayName}");
                            modules.Add(module);
                        }
                    }
                    catch (Exception e)
                    {
                        UptLogger.Error($"Failed to create module {type.Name}: {e.Message}");
                    }
                }
            }
            catch (ReflectionTypeLoadException e)
            {
                UptLogger.Error($"Failed to load types from assembly {assembly.GetName().Name}: {e.LoaderExceptions.First()?.Message}");
            }
            catch (Exception e)
            {
                UptLogger.Warning($"Failed to scan assembly {assembly.GetName().Name} for modules: {e.Message}");
            }

            return modules;
        }

        private void InitializeAndRegisterModules(IList<IPlatformModule> modules)
        {
            foreach (var module in modules)
            {
                try
                {
                    if (!module.IsAvailable())
                    {
                        UptLogger.Warning($"Module {GetModuleName(module)} isn't available, skipping...");
                        continue;
                    }

                    if (module.Initialize())
                    {
                        m_loadedModules.Add(module);
                        //UptLogger.Info($"Successfully loaded module: {GetModuleName(module)}");
                    }
                }
                catch (Exception e)
                {
                    UptLogger.Error($"Failed to initialize module {GetModuleName(module)}: {e.Message}");
                }
            }
        }

        private void FillMockServices()
        {
            if (m_loadedModules == null || m_loadedModules.Count == 0)
            {
                UptLogger.Warning("No modules loaded, cannot fill mock services");
                return;
            }

            UptLogger.Info("Starting mock service initialization...");

            var availableServiceTypes = ModuleUtility.GetAllAvailableServiceTypes(m_loadedModules);
            var missingCount = 0;
            var mockCount = 0;

            foreach (var serviceType in availableServiceTypes)
            {
                if (ServiceContainer.IsRegistered(serviceType))
                    continue;

                missingCount++;

                var mockService = MockServiceFactory.Create(serviceType);
                if (mockService != null)
                {
                    if (ServiceContainer.Register(serviceType, mockService))
                    {
                        UptLogger.Info($"Registered mock service for: {serviceType.Name}");
                        mockCount++;
                    }
                }
            }

            if (missingCount > mockCount)
                UptLogger.Warning($"{missingCount - mockCount} services are unavailable! Some game features may not work correctly.");
        }

        private void LogLoadingSummary()
        {
            var log = new StringBuilder("=================================\n" +
                "MODULE LOADING SUMMARY\n" +
                $"Total modules loaded: {m_loadedModules.Count}\n");

            foreach (var module in m_loadedModules)
                log.AppendLine($"   ✅ {GetModuleName(module)}");

            if (m_loadedModules.Count == 0)
            {
                log.AppendLine("No platform modules were loaded!");
                log.AppendLine("Check if:");
                log.AppendLine("   • Module packages are installed");
                log.AppendLine("   • Platform requirements are met");
                log.AppendLine("   • Modules are enabled in Module Manager");
            }

            var totalServices = ModuleUtility.GetAllAvailableServiceTypes(m_loadedModules).Count;
            log.AppendLine($"Total services loaded: {totalServices}");

            var servicesEnumerator = ServiceContainer.GetEnumerator();
            while (servicesEnumerator.MoveNext())
            {
                if (servicesEnumerator.Current is IMockService mock)
                    log.AppendLine($"   ⚠️ {mock.OriginalServiceName} (MOCK)");
                else
                    log.AppendLine($"   ✅ {servicesEnumerator.Current}");
            }

            log.Append($"=================================");
            Debug.Log(log.ToString());
        }

        private string GetModuleName(IPlatformModule module)
        {
            var moduleType = module.GetType();
            return module?.DisplayName ?? moduleType.Name;
        }
    }
}
