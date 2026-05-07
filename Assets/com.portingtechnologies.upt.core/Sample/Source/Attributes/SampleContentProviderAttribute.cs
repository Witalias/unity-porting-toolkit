using System;

namespace UPT.Core.Samples
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SampleContentProviderAttribute : Attribute
    {
        public string ServiceName { get; }

        public SampleContentProviderAttribute(string serviceName)
        {
            ServiceName = serviceName;
        }
    }
}
