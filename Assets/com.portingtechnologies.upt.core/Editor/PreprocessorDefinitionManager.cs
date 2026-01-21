using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;

namespace UPT.Editor
{
    public static class PreprocessorDefinitionManager
    {
        public static string[] GetCurrentDefinitions(NamedBuildTarget buildTargetGroup)
        {
            string defines = PlayerSettings.GetScriptingDefineSymbols(buildTargetGroup);
            return defines.Split(';')
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Select(d => d.Trim())
                .ToArray();
        }

        public static void SetDefinitions(NamedBuildTarget buildTargetGroup, string[] definitions)
        {
            string defines = string.Join(";", definitions.Where(d => !string.IsNullOrWhiteSpace(d)));
            PlayerSettings.SetScriptingDefineSymbols(buildTargetGroup, defines);
        }

        public static bool Set(NamedBuildTarget buildTargetGroup, string definition, bool enable)
        {
            if (string.IsNullOrWhiteSpace(definition))
                return false;

            definition = definition.Trim();
            var definitions = GetCurrentDefinitions(buildTargetGroup).ToList();

            if (enable && definitions.Contains(definition))
                return false;

            if (!enable && !definitions.Contains(definition))
                return false;

            if (enable)
                definitions.Add(definition);
            else
                definitions.Remove(definition);

            SetDefinitions(buildTargetGroup, definitions.ToArray());
            return true;
        }

        public static bool HasDefinition(NamedBuildTarget buildTargetGroup, string definition)
        {
            if (string.IsNullOrWhiteSpace(definition))
                return false;

            definition = definition.Trim();
            var definitions = GetCurrentDefinitions(buildTargetGroup);
            return definitions.Contains(definition);
        }
    }
}