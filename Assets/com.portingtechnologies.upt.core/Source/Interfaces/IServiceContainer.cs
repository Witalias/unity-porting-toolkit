using System;
using System.Collections.Generic;

namespace UPT.Core
{
    public interface IServiceContainer
    {
        IReadOnlyCollection<Type> GetProvidedServices();
        void RegisterService<T>(T service) where T : class;
        void UnregisterService<T>() where T : class;
    }
}
