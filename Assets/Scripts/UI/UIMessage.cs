using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using System;


namespace UIFramework
{
    /// <summary>
    /// 用于UI界面之间的消息传递
    /// </summary>
    public class UIMessage : Singleton<UIMessage>
    {
        private Dictionary<string, Delegate> m_UIMessageDic = new Dictionary<string, Delegate>();

        public void AddMsgLister(string eventType, Action handler)
        {
            if (OnHandlerAdding(eventType, handler))
            {
                m_UIMessageDic[eventType] = Delegate.Combine(m_UIMessageDic[eventType], handler);
            }
        }

        public void AddMsgLister<T>(string eventType, Action<T> handler)
        {
            if (OnHandlerAdding(eventType, handler))
            {
                m_UIMessageDic[eventType] = Delegate.Combine(m_UIMessageDic[eventType], handler);
            }
        }

        
        public void AddMsgLister<T1, T2>(string eventType, Action<T1, T2> handler)
        {
            if (OnHandlerAdding(eventType, handler))
            {
                m_UIMessageDic[eventType] = Delegate.Combine(m_UIMessageDic[eventType], handler);
            }
        }

        public void AddMsgLister<T1, T2, T3>(string eventType, Action<T1, T2, T3> handler)
        {
            if (OnHandlerAdding(eventType, handler))
            {
                m_UIMessageDic[eventType] = Delegate.Combine(m_UIMessageDic[eventType], handler);
            }
        }

        public void AddMsgLister<T1, T2, T3, T4>(string eventType, Action<T1, T2, T3, T4> handler)
        {
            if (OnHandlerAdding(eventType, handler))
            {
                m_UIMessageDic[eventType] = Delegate.Combine(m_UIMessageDic[eventType], handler);
            }
        }

        public void RemoveMsgListener(string eventType, Action handler)
        {
            if (OnHandlerRemoving(eventType, handler))
            {
                m_UIMessageDic[eventType] = Delegate.Remove(m_UIMessageDic[eventType], handler);
            }
        }

        public void RemoveMsgListener<T>(string eventType, Action<T> handler)
        {
            if (OnHandlerRemoving(eventType, handler))
            {
                m_UIMessageDic[eventType] = Delegate.Remove(m_UIMessageDic[eventType], handler);
            }
        }

        public void RemoveMsgListener<T1, T2>(string eventType, Action<T1, T2> handler)
        {
            if (OnHandlerRemoving(eventType, handler))
            {
                m_UIMessageDic[eventType] = Delegate.Remove(m_UIMessageDic[eventType], handler);
            }
        }

        public void RemoveMsgListener<T1, T2, T3>(string eventType, Action<T1, T2, T3> handler)
        {
            if (OnHandlerRemoving(eventType, handler))
            {
                m_UIMessageDic[eventType] = Delegate.Remove(m_UIMessageDic[eventType], handler);
            }
        }

        public void RemoveMsgListener<T1, T2, T3, T4>(string eventType, Action<T1, T2, T3, T4> handler)
        {
            if (OnHandlerRemoving(eventType, handler))
            {
                m_UIMessageDic[eventType] = Delegate.Remove(m_UIMessageDic[eventType], handler);
            }
        }

        public void BroadCastEvent(string eventType)
        {
            if (OnBroadCasting(eventType))
            {
                Action aciton = m_UIMessageDic[eventType] as Action;
                if (aciton != null)
                {
                    aciton();
                }
            }
        }

        public void BroadCastEvent<T1>(string eventType, T1 arg1)
        {
            if (OnBroadCasting(eventType))
            {
                Action<T1> aciton = m_UIMessageDic[eventType] as Action<T1>;
                if (aciton != null)
                {
                    aciton(arg1);
                }
            }
        }

        public void BroadCastEvent<T1, T2>(string eventType, T1 arg1, T2 arg2)
        {
            if (OnBroadCasting(eventType))
            {
                Action<T1, T2> aciton = m_UIMessageDic[eventType] as Action<T1, T2>;
                if (aciton != null)
                {
                    aciton(arg1, arg2);
                }
            }
        }

        public void BroadCastEvent<T1, T2, T3>(string eventType, T1 arg1, T2 arg2, T3 arg3)
        {
            if (OnBroadCasting(eventType))
            {
                Action<T1, T2, T3> aciton = m_UIMessageDic[eventType] as Action<T1, T2, T3>;
                if (aciton != null)
                {
                    aciton(arg1, arg2, arg3);
                }
            }
        }

        public void BroadCastEvent<T1, T2, T3, T4>(string eventType, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            if (OnBroadCasting(eventType))
            {
                Action<T1, T2, T3, T4> aciton = m_UIMessageDic[eventType] as Action<T1, T2, T3, T4>;
                if (aciton != null)
                {
                    aciton(arg1, arg2, arg3, arg4);
                }
            }
        }

        public bool OnHandlerAdding(string eventType, Delegate handler)
        {
            bool result = true;
            if (!m_UIMessageDic.ContainsKey(eventType))
            {
                m_UIMessageDic.Add(eventType, null);
            }
            Delegate del = m_UIMessageDic[eventType];
            if (del != null && del.GetType() != handler.GetType())
            {
                result = false;
            }
            return result;
        }

        private bool OnHandlerRemoving(string eventType, Delegate handler)
        {
            bool result = true;
            if (m_UIMessageDic.ContainsKey(eventType))
            {
                Delegate del = m_UIMessageDic[eventType];
                if (del != null)
                {
                    if (del.GetType() != handler.GetType())
                    {
                        result = false;
                    }
                }
                else
                {
                    result = false;
                }

            }
            else
            {
                result = false;
            }
            return result;
        }

        private bool OnBroadCasting(string eventType)
        {
            if (m_UIMessageDic.ContainsKey(eventType))
            {
                return true;
            }
            return false;
        }

        public void ClearAllMsgListener()
        {
            if (m_UIMessageDic != null)
            {
                m_UIMessageDic.Clear();
            }
        }
    }
}
