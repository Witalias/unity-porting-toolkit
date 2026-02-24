using UnityEditor;
using UnityEngine;

namespace UPT.Editor
{
    public static class GUIConstants
    {
        public static class Gaps
        {
            public static float Padding => 10.0f;
            public static float SectionGap => 10.0f;
            public static float IconButtonWidth => 30.0f;
        }

        public static class Styles
        {
            public static GUIStyle TitleLabel { get; }
            public static GUIStyle GrayLabel { get; }
            public static GUIStyle GreenLabel { get; }
            public static GUIStyle RedLabel { get; }

            static Styles()
            {
                TitleLabel = new();
                TitleLabel.fontSize = 16;
                TitleLabel.fontStyle = FontStyle.Bold;

                GrayLabel = new(EditorStyles.label);
                GrayLabel.normal.textColor = Color.gray;

                GreenLabel = new(EditorStyles.label);
                GreenLabel.normal.textColor = Color.darkGreen;

                RedLabel = new(EditorStyles.label);
                RedLabel.normal.textColor = Color.darkRed;
            }
        }

        public static class Icons
        {
            public static string Trash => "TreeEditor.Trash";
            public static string Refresh => "Refresh";
            public static string Plus => "Toolbar Plus";
            public static string Info => "UnityEditor.InspectorWindow";
            public static string Save => "SaveAs";
            public static string Tools => "CustomTool";
            public static string Settings => "Settings";
            public static string Edit => "editicon.sml";
            public static string Loading => "Loading";
        }

        public static Texture2D GetIcon(string iconName)
        {
            return EditorGUIUtility.IconContent(iconName).image as Texture2D;
        }
    }
}
