using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UPT.Core.AssetConverter
{
    public static class Utils
    {
        public static PathComparer s_pathComparer = new();

        public static TextureType ConvertToTextureType(TextureImporterType type, TextureImporterShape shape)
        {
            TextureType result;
            if (type is TextureImporterType.Default)
                result = shape is TextureImporterShape.Texture2D ? TextureType.Default2D : TextureType.DefaultCubemap;
            else
                result = type is TextureImporterType.Sprite ? TextureType.Sprite : TextureType.Lightmap;

            return result;
        }

        public static (TextureImporterType, TextureImporterShape) ConvertToTextureImporter(TextureType type)
        {
            TextureImporterType importerType;
            TextureImporterShape importerShape = TextureImporterShape.Texture2D;
            if (type is TextureType.Default2D || type is TextureType.DefaultCubemap)
            {
                importerType = TextureImporterType.Default;
                importerShape = type is TextureType.Default2D ? TextureImporterShape.Texture2D : TextureImporterShape.TextureCube;
            }
            else
            {
                importerType = type is TextureType.Lightmap ? TextureImporterType.Lightmap : TextureImporterType.Sprite;
            }
            return (importerType, importerShape);
        }

        public static AlphaSourcePresence ConvertToAlphaSourcePresence(TextureImporterAlphaSource alphaSource) => 
            alphaSource is TextureImporterAlphaSource.None ? AlphaSourcePresence.None : AlphaSourcePresence.Assigned;

        public static Texture2D GetUnityIcon(string name) => (Texture2D)EditorGUIUtility.IconContent(name).image;

        public static void AddSubdirectories(IList<string> directoryPaths)
        {
            for (var i = 0; i < directoryPaths.Count; i++)
            {
                var path = directoryPaths[i];

                if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                    continue;

                var subDirectories = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
                for (var j = 0; j < subDirectories.Length; j++)
                {
                    subDirectories[j] = subDirectories[j].Replace('\\', '/');
                    if (!directoryPaths.Contains(subDirectories[j]))
                        directoryPaths.Add(subDirectories[j]);
                }
            }
        }

        public static List<string> RemoveSubdirectories(List<string> directoryPaths, IList<string> removedDirectories)
        {
            var updatedList = new List<string>(directoryPaths);
            foreach (var path in directoryPaths)
            {
                if (!removedDirectories.Contains(path, s_pathComparer))
                    continue;

                if (updatedList.Contains(path))
                    updatedList.Remove(path);

                var subDirectories = directoryPaths.FindAll(directory => directory.StartsWith(path, StringComparison.OrdinalIgnoreCase));
                foreach (var subDirectory in subDirectories)
                {
                    if (updatedList.Contains(subDirectory))
                        updatedList.Remove(subDirectory);
                }
            }
            return updatedList;
        }

        public static void DrawDropdownMenu(ICollection content, object currentOption, GenericMenu.MenuFunction2 onOptionClick)
        {
            var menu = new GenericMenu();
            foreach (var option in content)
                menu.AddItem(new GUIContent(option.ToString()), false, onOptionClick, option);

            if (EditorGUILayout.DropdownButton(new GUIContent(currentOption.ToString()), FocusType.Keyboard))
                menu.ShowAsContext();
        }

        public class PathComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y)
            {
                return x.Equals(y, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(string obj)
            {
                return obj.ToLower().GetHashCode();
            }
        }
    }
}
