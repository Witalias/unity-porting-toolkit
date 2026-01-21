using UnityEngine;
using TMPro;
using System.Text;
using System.Collections.Generic;
using System;

namespace UPT.Core
{
    public class RuntimeDebugConsole : MonoBehaviour
    {
        private const int MAX_LOG_LINES = 500;

        [SerializeField] private TMP_InputField m_inputField;

        private StringBuilder m_logBuilder = new();
        private Queue<string> m_logQueue = new();

        private void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (m_inputField == null)
                return;

            var newLog = type switch
            {
                LogType.Warning => $"<color=yellow>{logString}</color>\n",
                LogType.Error or LogType.Exception or LogType.Assert => $"<color=red>{logString}</color>\n",
                _ => $"<color=white>{logString}</color>\n",
            };
            newLog = newLog.Insert(0, $"[{DateTime.Now}] ");
            m_logQueue.Enqueue(newLog);

            if (type == LogType.Exception)
                m_logQueue.Enqueue(stackTrace + "\n");

            while (m_logQueue.Count > MAX_LOG_LINES)
                m_logQueue.Dequeue();

            m_logBuilder.Clear();
            foreach (string log in m_logQueue)
                m_logBuilder.Append(log);

            m_inputField.text = m_logBuilder.ToString();

            if (m_inputField.verticalScrollbar != null)
                m_inputField.verticalScrollbar.value = 1.0f;
        }
    }
}
