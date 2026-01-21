using System;

namespace UPT.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PlatformModuleAttribute : Attribute
    {
        public string PackageName { get; }
        public string ProprocessorDefinition { get; }
        public bool ProprocessorDefinitionInverted { get; }

        public PlatformModuleAttribute(string packageName, string preprocessorDefinition = null, bool definitionInverted = false)
        {
            PackageName = packageName;
            ProprocessorDefinition = preprocessorDefinition;
            ProprocessorDefinitionInverted = definitionInverted;
        }
    }
}
