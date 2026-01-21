using UnityEditor;
using UnityEngine;

namespace UPT.Core.AssetConverter
{
    public partial class AssetConverterWindow
    {
        private AssetTreeView m_streamingAudioClipsTreeView;
        private AssetTreeView m_longDecompressOnLoadAudioClipsTreeView;

        private void OptimizationModule()
        {
            DrawBuildTargets(UpdateLayout);

            EditorGUILayout.HelpBox("Click the button below to get performance optimization recommendations.", MessageType.Info);
            if (GUILayout.Button("Scan Project"))
            {
                m_streamingAudioClipsTreeView = new StreamingAudioTreeView();
                m_longDecompressOnLoadAudioClipsTreeView = new LongDecompressOnLoadAudioTreeView();
            }
            EditorGUILayout.Space(20.0f);

            var oldColor = GUI.backgroundColor;
            var storage = UserSettings.Instance;
            if (m_streamingAudioClipsTreeView != null && m_streamingAudioClipsTreeView.AssetsFound)
            {
                if (DrawFoldout(OptimizationSection.StreamingAudio, "Streaming Audio Clips"))
                {
                    EditorGUILayout.LabelField("Found audio clips with Load Type <b>Streaming</b>. Streaming audio is <color=green>not loaded into memory</color>, " +
                        "but read directly from disk, <color=red>significantly increasing CPU overhead</color>. " +
                        "Multiple streaming audio clips playing simultaneously can cause <color=red>memory fragmentation</color>.", m_generalTextStyle);
                    DrawSeparator();
                    EditorGUILayout.LabelField("Recommendations:\n" +
                        "1. Profile CPU overhead during streaming audio playback by monitoring <b>Streaming CPU</b> in <b>Unity Profiler (Audio Module)</b>.\n" +
                        "2. Make sure requests to these audio clips are not made too often.\n" +
                        "3. Make sure there are no situations where multiple streaming audio clips are playing at the same time.\n" +
                        "4. If you are not satisfied with the CPU overhead of streaming audio, consider changing Load Type to <b>Compressed In Memory</b> or <b>Decompress On Load</b>", m_generalTextStyle);
                    m_streamingAudioClipsTreeView.Draw();
                    EditorGUILayout.Space(20.0f);
                }
            }

            if (m_longDecompressOnLoadAudioClipsTreeView != null && m_longDecompressOnLoadAudioClipsTreeView.AssetsFound)
            {
                if (DrawFoldout(OptimizationSection.LongDecompressOnLoadAudio, "Decompress on Load Audio Clips"))
                {
                    EditorGUILayout.LabelField("Very long audio clips (longer than 30 seconds) with Load Type <b>Decompress On Load</b> were found. " +
                        "Although such audio clips are read with <color=green>minimal CPU overhead</color>, they can <color=red>significantly increase memory consumption</color> " +
                        "and their decompression in memory can <color=red>take a long time</color>.", m_generalTextStyle);
                    DrawSeparator();
                    EditorGUILayout.LabelField("Recommendations:\n" +
                        "1. Profile the memory consumption of decompressed audio clips in <b>Unity Memory Profiler</b> or by monitoring <b>Sample Sound Memory</b> in <b>Unity Profiler (Audio Module)</b>.\n" +
                        "2. If memory is limited, consider changing Load Type to <b>Compressed In Memory</b> or <b>Streaming</b>.", m_generalTextStyle);
                    m_longDecompressOnLoadAudioClipsTreeView.Draw();
                    EditorGUILayout.Space(20.0f);
                }
            }

            void UpdateLayout()
            {
                m_streamingAudioClipsTreeView = null;
            }

            bool DrawFoldout(OptimizationSection section, string name)
            {
                var foldoutData = storage.GetOptimizationModuleFoldoutData(section);
                GUI.backgroundColor = foldoutData.Completed ? m_highlightBackgroundColor3 : oldColor;
                EditorGUILayout.BeginHorizontal();
                foldoutData.Opened = EditorGUILayout.Foldout(foldoutData.Opened, name, true, "FoldoutHeader");
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(new GUIContent("Completed", "Mark as completed"), GUILayout.Width(70.0f));
                EditorGUI.BeginChangeCheck();
                foldoutData.Completed = EditorGUILayout.Toggle(foldoutData.Completed, GUILayout.Width(18.0f));
                if (EditorGUI.EndChangeCheck() && foldoutData.Completed)
                    foldoutData.Opened = false;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
                GUI.backgroundColor = oldColor;
                return foldoutData.Opened;
            }
        }
    }
}
