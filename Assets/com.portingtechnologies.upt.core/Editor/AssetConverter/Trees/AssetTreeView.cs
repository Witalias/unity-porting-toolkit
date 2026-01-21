using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UPT.Core.AssetConverter
{
    public abstract class AssetTreeView : TreeView
    {
        private readonly System.Type m_assetType;
        private readonly bool m_overrideOption;

        public bool AssetsFound { get; private set; }
        public int AssetCount { get; private set; }

        public AssetTreeView(System.Type assetType, bool overrideOption) : base(new TreeViewState())
        {
            m_assetType = assetType;
            m_overrideOption = overrideOption;

            var columns = new List<MultiColumnHeaderState.Column>
            {
                new() {
                    headerContent = new GUIContent("Name"),
                    canSort = false,
                    minWidth = 250.0f,
                    width = 250.0f
                }
            };
            if (overrideOption)
            {
                columns.Add(new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Override"),
                    canSort = false,
                    minWidth = 80.0f,
                    width = 80.0f
                });
            }
            var multiColumnState = new MultiColumnHeaderState(columns.ToArray());
            multiColumnHeader = new MultiColumnHeader(multiColumnState);
        }

        protected sealed override TreeViewItem BuildRoot()
        {
            var assetIconName = GetAssetIconName();
            if (assetIconName == null)
                Debug.LogError($"Failure to get icon name for type {m_assetType}");

            var root = new TreeViewItem(-1, -1);
            var rootDirectoryItem = new DirectoryItem(new DirectoryInfo(Application.dataPath), m_assetType, assetIconName, IsValidAsset);
            root.AddChild(rootDirectoryItem);

            var fullTypeName = m_assetType.ToString();
            var typeName = fullTypeName[(fullTypeName.LastIndexOf('.') + 1)..];
            var guids = AssetDatabase.FindAssets($"t:{typeName}", new[] { "Assets" });
            var assetPaths = new List<string>();
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (assetPath != null)
                    assetPaths.Add(assetPath);
            }

            foreach (var path in assetPaths)
            {
                if (!IsValidAsset(path))
                    continue;

                var asset = AssetDatabase.LoadAssetAtPath(path, m_assetType);
                if (FindItem(asset.GetHashCode(), root) != null)
                    continue;

                var directory = new DirectoryInfo(Path.GetDirectoryName(path));
                var directories = new List<DirectoryInfo> { directory };
                while (directory.Parent.Name != "Assets")
                {
                    directory = new DirectoryInfo(directory.Parent.FullName[directory.FullName.IndexOf("Assets")..]);
                    directories.Add(directory);
                }
                directories.Reverse();

                var directoryItems = new List<DirectoryItem>();
                var parentItem = rootDirectoryItem as TreeViewItem;
                foreach (var dir in directories)
                {
                    var directoryItem = FindItem(dir.GetHashCode(), parentItem);
                    if (directoryItem == null)
                    {
                        directoryItem = new DirectoryItem(dir, m_assetType, assetIconName, IsValidAsset);
                        directoryItems.Add(directoryItem as DirectoryItem);
                        parentItem.AddChild(directoryItem);
                        AssetCount += ((DirectoryItem)directoryItem).AssetCount;
                    }
                    parentItem = directoryItem;
                }

                foreach (var item in directoryItems)
                    item.CheckOverrideFlag();
            }

            AssetsFound = rootDirectoryItem.hasChildren;
            if (rootDirectoryItem.hasChildren)
                rootDirectoryItem.CheckOverrideFlag();

            showAlternatingRowBackgrounds = true;
            showBorder = true;
            SetupDepthsFromParentsAndChildren(root);

            return root;
        }

        protected sealed override void RowGUI(RowGUIArgs args)
        {
            base.RowGUI(args);

            if (m_overrideOption)
            {
                var rect = args.rowRect;
                rect.x = multiColumnHeader.GetColumnRect(0).width;
                rect.width = multiColumnHeader.GetColumnRect(1).width;

                var item = args.item;
                if (item is ITreeItem treeItem)
                {
                    EditorGUI.BeginChangeCheck();
                    var overrideFlag = EditorGUI.Toggle(rect, treeItem.Override);
                    if (EditorGUI.EndChangeCheck())
                        treeItem.Override = overrideFlag;
                }
            }
        }

        protected sealed override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);

            if (selectedIds.Count == 0)
                return;

            var item = FindItem(selectedIds[0], rootItem);
            if (item is not ITreeItem treeItem)
                return;

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = treeItem.Source;
        }

        protected sealed override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected abstract string GetAssetIconName();

        protected abstract bool IsValidAsset(string path);

        protected bool IsValidAsset(Object asset)
        {
            if (asset == null)
                return false;

            return IsValidAsset(AssetDatabase.GetAssetPath(asset));
        }

        public void Draw()
        {
            var rect = EditorGUILayout.GetControlRect(false, 250.0f);
            //rect = EditorGUI.IndentedRect(rect);
            OnGUI(rect);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Expand All", "miniButton"))
            {
                ExpandAll();
            }
            if (GUILayout.Button("Collapse All", "miniButton"))
            {
                CollapseAll();
            }
            EditorGUILayout.EndHorizontal();
        }

        private class DirectoryItem : TreeViewItem, ITreeItem
        {
            private const string DIRECTORY_ICON = "Folder Icon";

            private bool m_override = true;

            public bool Override
            {
                get => m_override;
                set
                {
                    m_override = value;

                    if (!hasChildren)
                        return;

                    foreach (var child in children)
                    {
                        if (child is ITreeItem treeItem)
                            treeItem.Override = value;
                    }
                }
            }

            public Object Source { get; }

            public int AssetCount { get; private set; }

            public DirectoryItem(DirectoryInfo directoryInfo, System.Type assetType, string assetIconName, System.Func<Object, bool> isValidAsset)
                : base(directoryInfo.GetHashCode(), 0, directoryInfo.Name)
            {
                icon = Utils.GetUnityIcon(DIRECTORY_ICON);

                var directoryPath = ConvertAbsolutePathsToRelative(directoryInfo.FullName).First();
                Source = AssetDatabase.LoadAssetAtPath<Object>(directoryPath);

                var directoryData = UserSettings.Instance.GetAssetData(directoryPath);
                if (directoryData != null)
                    m_override = directoryData.OverrideSettings;

                var assetPaths = ConvertAbsolutePathsToRelative(directoryInfo.GetFiles().Select(file => file.FullName).ToArray());
                foreach (var path in assetPaths)
                {
                    var asset = AssetDatabase.LoadAssetAtPath(path, assetType);
                    if (isValidAsset(asset))
                    {
                        AddChild(new AssetItem(asset, assetIconName));
                        AssetCount++;
                    }
                }
            }

            public void CheckOverrideFlag()
            {
                if (!hasChildren)
                    return;

                foreach (var child in children)
                {
                    if (child is ITreeItem treeItem && treeItem.Override)
                    {
                        m_override = true;
                        return;
                    }
                }
                m_override = false;
            }

            private IEnumerable<string> ConvertAbsolutePathsToRelative(params string[] paths) => paths.Select(path => path[path.IndexOf("Assets")..]);
        }

        private class AssetItem : TreeViewItem, ITreeItem
        {
            private bool m_override = true;

            public bool Override
            {
                get => m_override;
                set
                {
                    m_override = value;
                    UserSettings.Instance.UpdateAssetData(this);
                }
            }

            public Object Source { get; }

            public AssetItem(Object source, string iconName) : base(source.GetHashCode(), 0, source.name)
            {
                Source = source;

                var assetData = UserSettings.Instance.GetAssetData(AssetDatabase.GetAssetPath(source));
                if (assetData != null)
                    m_override = assetData.OverrideSettings;

                if (iconName != null)
                    icon = Utils.GetUnityIcon(iconName);
            }
        }
    }


}
