using UnityEditor;
using UnityEngine;

namespace UPT.Editor
{
    public class GUILoading
    {
        private const float ANIMATION_SPEED = 0.0005f;

        private float m_animationTime;

        public GUILoading()
        {
            EditorApplication.update += UpdateRotation;
        }

        ~GUILoading()
        {
            EditorApplication.update -= UpdateRotation;
        }

        public void Draw()
        {
            var rect = EditorGUILayout.GetControlRect(GUILayout.Height(20));
            EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f, 0.3f));

            // Границы для бегущей части (не выходит за фон)
            var runnerWidth = Mathf.Min(50f, rect.width * 0.15f); // 15% ширины или 30px
            var maxPos = rect.width - runnerWidth;

            // Анимация движения (0-1 цикл)
            var normalizedPos = Mathf.PingPong(m_animationTime, 1f);
            var runnerX = rect.x + normalizedPos * maxPos;

            // Рисуем бегущую часть
            var runnerRect = new Rect(runnerX, rect.y, runnerWidth, rect.height);
            EditorGUI.DrawRect(runnerRect, new Color(0.1f, 0.5f, 1f, 0.8f));
        }

        private void UpdateRotation()
        {
            m_animationTime += ANIMATION_SPEED;

            if (EditorWindow.focusedWindow != null)
                EditorWindow.focusedWindow.Repaint();
        }
    }
}
