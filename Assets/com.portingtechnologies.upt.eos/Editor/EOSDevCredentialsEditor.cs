#if !EOS_DISABLE

using System.IO;
using UnityEditor;
using UnityEngine;
using UPT.EOS;

namespace UPT.Editor
{
    public class EOSDevCredentialsEditor : EditorWindow
    {
        private string m_host = "";
        private string m_username = "";

        [MenuItem("Tools/Porting Toolkit/EOS/Create developer credentials")]
        public static void ShowWindow()
        {
            var window = GetWindow<EOSDevCredentialsEditor>("EOS Developer Credentials");
            window.minSize = new Vector2(450, 190);
            window.maxSize = new Vector2(450, 190);
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox("To use the features of EOS in the editor, you should provide the host " +
                "and your user name configured in the Dev Auth Tool.\nFollow the Epic Online Services documentation to " +
                "learn about the Dev Auth Tool.", MessageType.Info);

            GUILayout.Space(GUIConstants.Gaps.SectionGap);

            m_host = EditorGUILayout.TextField("Host", m_host);
            m_username = EditorGUILayout.TextField("Username", m_username);

            GUILayout.Space(GUIConstants.Gaps.SectionGap);

            if (GUILayout.Button("Create", GUILayout.Height(30)))
            {
                CreateCredentialsFile();
            }

            GUILayout.Space(GUIConstants.Gaps.SectionGap);

            GUILayout.Label($"Credentials will be saved to:\n{EOSPlatformModule.DEV_CREDENTIALS_PATH}");
        }

        private void CreateCredentialsFile()
        {
            if (string.IsNullOrEmpty(m_host) || string.IsNullOrEmpty(m_username))
            {
                EditorUtility.DisplayDialog("Error", "Please fill in both fields", "OK");
                return;
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(EOSPlatformModule.DEV_CREDENTIALS_PATH));
                File.WriteAllText(EOSPlatformModule.DEV_CREDENTIALS_PATH, $"{m_host},{m_username}");

                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Success", $"Credentials saved to:\n{EOSPlatformModule.DEV_CREDENTIALS_PATH}\n\n" +
                    $"We recommend adding this file to .gitignore to avoid making personal data publicly available.", "OK");
                Close();

                m_host = "";
                m_username = "";
            }
            catch (IOException e)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to save credentials: {e.Message}", "OK");
            }
        }
    }
}

#endif