using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UGU.Runtime
{
    /// <summary>
    /// UGU MonoBehaviour 单例容器(Container)
    /// </summary>
    public class UGUMBSContainer : MonoBehaviour
    {
        private Dictionary<Type, UGUMBSingletonBase> m_singletonDic = new();

        private static UGUMBSContainer Instance;
        private static MethodInfo OnCreateMBSingleton;

        public static T Create<T>() where T : UGUMBSingletonBase
        {
            if (Instance == null)
            {
                GameObject container = new GameObject("UGU-MBSC");
                DontDestroyOnLoad(container);
                Instance = container.AddComponent<UGUMBSContainer>();
            }

            if (OnCreateMBSingleton == null)
            {
                OnCreateMBSingleton = UGUCommonUtils.GetObjectNoPublicMethod(typeof(UGUMBSingletonBase), "OnCreate");
            }

            T singleton = null;

            Type sKey = typeof(T);

            if (!Instance.m_singletonDic.ContainsKey(sKey))
            {
                GameObject newMBSGO = new GameObject(sKey.Name);
                UGUMBSingletonBase newMBS = newMBSGO.AddComponent<T>();
                newMBSGO.transform.SetParent(Instance.transform);
                Instance.m_singletonDic.Add(sKey, newMBS);
                singleton = newMBS as T;

                OnCreateMBSingleton?.Invoke(newMBS, null);
            }
            else
            {
                singleton = Instance.m_singletonDic[sKey] as T;
            }

            return singleton;
        }

        public static T Get<T>() where T : UGUMBSingletonBase
        {
            if (Instance == null)
            {
                Debug.LogWarning("[UGUMBSContainer] Get — container not initialized. Call Create<T>() first.");
                return null;
            }

            Type sKey = typeof(T);
            if (Instance.m_singletonDic.TryGetValue(sKey, out UGUMBSingletonBase mbs))
            {
                return mbs as T;
            }

            return null;
        }
    }
}
