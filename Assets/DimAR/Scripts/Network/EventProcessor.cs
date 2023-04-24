using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
namespace GRT
{
    public class EventProcessor : MonoBehaviour
    {
        private System.Object m_queueLock = new System.Object();
        private List<Action<string>> m_queuedEvents = new List<Action<string>>();
        private List<Action<string>> m_executingEvents = new List<Action<string>>();
        private string message;

        public void QueueEvent(Action<string> action, string msg)
        {
            lock (m_queueLock)
            {
                m_queuedEvents.Add(action);
                message = msg;
            }
        }

        void Update()
        {
            MoveQueuedEventsToExecuting();
            
            while (m_executingEvents.Count > 0)
            {
                Action<string> e = m_executingEvents[0];
                m_executingEvents.RemoveAt(0); 
                e(message);
            }
        }

        private void MoveQueuedEventsToExecuting()
        {
            lock (m_queueLock)
            {
                while (m_queuedEvents.Count > 0)
                {
                    Action<string> e = m_queuedEvents[0];
                    m_executingEvents.Add(e);
                    m_queuedEvents.RemoveAt(0);
                }
            }
        }
    }

}