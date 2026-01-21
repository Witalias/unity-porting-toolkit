using System;
using UnityEngine;

namespace UPT.Core
{
    public class DelayedAction
    {
        private readonly float m_delay;
        private readonly Action m_action;

        private float m_remains;
        private bool m_executeAfterExpiration;

        public DelayedAction(float delay, Action action)
        {
            m_delay = delay;
            m_action = action;
        }

        public void Run(bool executeImmediatelyIfPossible = false)
        {
            if (m_remains <= 0.0f)
            {
                m_remains = m_delay;

                if (executeImmediatelyIfPossible)
                    OnExpired();
                else
                    m_executeAfterExpiration = true;
            }
            else
            {
                m_executeAfterExpiration = true;
            }
        }

        public void Abort()
        {
            m_remains = 0.0f;
            m_executeAfterExpiration = false;
        }

        public void Update()
        {
            if (m_remains > 0.0f)
            {
                m_remains -= Time.deltaTime;
                if (m_remains <= 0.0f)
                {
                    m_remains = 0.0f;
                    if (m_executeAfterExpiration)
                    {
                        m_remains = m_delay;
                        m_executeAfterExpiration = false;
                        OnExpired();
                    }
                }
            }
        }

        private void OnExpired()
        {
            m_action?.Invoke();
        }
    }
}
