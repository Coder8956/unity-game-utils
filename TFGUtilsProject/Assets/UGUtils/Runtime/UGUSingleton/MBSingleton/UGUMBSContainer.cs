using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UGU.Runtime
{
    /// <summary>
    /// UGU MonoBehaviour 单例容器(Container)
    /// </summary>
    public class UGUMBSC : MonoBehaviour
    {
        private static UGUMBSC Inst = null;
        private static MethodInfo OnCreateMBSingleton = null;

        public static T Create<T>() where T : BaseMBSingleton
        {
            if (Inst == null)
            {
                GameObject container = new GameObject("UGU-MBSC");
                DontDestroyOnLoad(container);
                Inst = container.AddComponent<UGUMBSC>();
            }

            if (OnCreateMBSingleton == null)
            {
                OnCreateMBSingleton = UGUtilCommon.GetObjectNoPublicMethod(typeof(BaseMBSingleton), "OnCreate");
            }

            T singleton = null;

            Type sKey = typeof(T);

            if (!Inst.m_singletonDic.ContainsKey(sKey))
            {
                GameObject newMBSGO = new GameObject(sKey.Name);
                BaseMBSingleton newMBS = newMBSGO.AddComponent<T>();
                newMBSGO.transform.SetParent(Inst.transform);
                Inst.m_singletonDic.Add(sKey, newMBS);
                singleton = newMBS as T;

                OnCreateMBSingleton?.Invoke(newMBS, null);
            }

            return singleton as T;
        }

        public static T Get<T>() where T : BaseMBSingleton
        {
            Type sKey = typeof(T);
            BaseMBSingleton MBS = null;
            if (Inst.m_singletonDic.ContainsKey(sKey))
            {
                MBS = Inst.m_singletonDic[sKey];
            }

            return MBS as T;
        }

        private Dictionary<Type, BaseMBSingleton> m_singletonDic = new();
    }
}