using System;
using System.Collections.Generic;
using UnityEngine;

namespace UGUtils.Runtime.UGUBD
{
    public class UGUBD<T>
    {
        private T m_value;
        private List<Action<T>> m_subscribers = new();
        public Func<T, T> OnModifyValue { get; set; }

        public T Value
        {
            get { return m_value; }
            set
            {
                if (OnModifyValue != null)
                {
                    m_value = OnModifyValue.Invoke(value);
                }
                else
                {
                    m_value = value;
                }

                for (var i = m_subscribers.Count - 1; i >= 0; i--)
                {
                    Action<T> sub = m_subscribers[i];
                    sub.Invoke(m_value);
                }
            }
        }

        public void Subscribe(Action<T> subscriber)
        {
            if (!m_subscribers.Contains(subscriber))
            {
                m_subscribers.Add(subscriber);
                subscriber.Invoke(m_value);
            }
        }

        public void Unsubscribe(Action<T> subscriber)
        {
            m_subscribers.Remove(subscriber);
        }

        public void ClearAllSubscribe()
        {
            m_subscribers.Clear();
        }
    }
}