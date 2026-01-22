using UnityEditor;
using UnityEngine;

namespace UPT.Editor
{
    public static class GUILayoutRunningBar
    {
        private const float ANIMATION_SPEED = 0.001f;

        private static float m_animationTime;

        public static void Draw()
        {
            m_animationTime += ANIMATION_SPEED;

            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(20));
            EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f, 0.3f));

            // Границы для бегущей части (не выходит за фон)
            float runnerWidth = Mathf.Min(50f, rect.width * 0.15f); // 15% ширины или 30px
            float maxPos = rect.width - runnerWidth;

            // Анимация движения (0-1 цикл)
            float normalizedPos = Mathf.PingPong(m_animationTime, 1f);
            float runnerX = rect.x + normalizedPos * maxPos;

            // Рисуем бегущую часть
            Rect runnerRect = new Rect(runnerX, rect.y, runnerWidth, rect.height);
            EditorGUI.DrawRect(runnerRect, new Color(0.1f, 0.5f, 1f, 0.8f));

            if (Event.current.type == EventType.Repaint && EditorWindow.focusedWindow != null)
                EditorWindow.focusedWindow.Repaint();
        }
    }
}
