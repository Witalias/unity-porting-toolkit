using System;

namespace UPT.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MockServiceAttribute : Attribute
    {
        public Type ServiceType { get; }

        public MockServiceAttribute(Type serviceType = null)
        {
            ServiceType = serviceType;
        }
    }
}
