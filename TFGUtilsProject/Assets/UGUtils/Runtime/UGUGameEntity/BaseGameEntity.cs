using System;
using System.Collections.Generic;
using System.Reflection;
using UGU.Runtime.Utils;
using UnityEngine;

namespace UGUtils.Runtime.UGUGameEntity
{
    public abstract class BaseGameEntity<T> : MonoBehaviour where T : BaseGameEntity<T>
    {
        private static T Instance = null;
        private static MethodInfo OnSystemInit = null;

        private Dictionary<Type, BaseGameSystem> m_dicSystems;

        protected Dictionary<Type, BaseGameSystem> DicSystems
        {
            get { return m_dicSystems; }
        }

        public static void Init(string name = "GameEntity")
        {
            if (Instance == null)
            {
                OnSystemInit = UGUtilCommon.GetObjectNoPublicMethod(typeof(BaseGameSystem), "OnCreate");
                GameObject gameEntity = new GameObject(name);
                Instance = gameEntity.AddComponent<T>();
                DontDestroyOnLoad(gameEntity);
                Instance.OnInit();
                Instance.m_dicSystems = new Dictionary<Type, BaseGameSystem>();
            }
        }

        protected abstract void OnInit();

        /// <summary>
        /// 添加游戏系统
        /// </summary>
        /// <typeparam name="S"></typeparam>
        public static void Add<S>() where S : BaseGameSystem
        {
            Type key = typeof(S);
            if (Instance.m_dicSystems.ContainsKey(key))
            {
                Debug.LogWarning($"Repeated addition system <{key.FullName}>.");
                return;
            }

            GameObject newSys = new GameObject($"System-{key.Name}");
            newSys.transform.SetParent(Instance.transform);
            BaseGameSystem newSysComp = newSys.AddComponent<S>();
            Instance.m_dicSystems.Add(typeof(S), newSysComp);
            OnSystemInit?.Invoke(newSysComp, null);
        }

        /// <summary>
        /// 获取游戏系统
        /// </summary>
        /// <typeparam name="S"></typeparam>
        public static S Get<S>() where S : BaseGameSystem
        {
            Type key = typeof(S);
            S sys = null;
            if (Instance.m_dicSystems.ContainsKey(key))
            {
                sys = Instance.m_dicSystems[key] as S;
            }
            else
            {
                Debug.LogWarning($"Game system <{key.FullName}> does not exist.");
            }

            return sys;
        }
    }
}