using System;
using System.Collections.Generic;

namespace UPT.Core
{
    public interface IPlatformModule
    {
        string DisplayName { get; }
        string Version { get; }
        IReadOnlyCollection<Type> ProvidedServiceTypes { get; }

        bool Initialize();
        void Shutdown();
        bool IsAvailable();

        T GetService<T>() where T : class;
        bool IsServiceSupported<T>() where T : class;
    }
}