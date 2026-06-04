using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UGU.Runtime.Utils
{
    public static class UGUtilCommon
    {
        /// <summary>
        /// 转换对象类型(class)
        /// </summary>
        /// <param name="obj">对象</param>
        /// <typeparam name="T">目标类型</typeparam>
        /// <returns>转换失败返回 null</returns>
        public static T ConvertObjectClass<T>(object obj) where T : class
        {
            if (obj is T)
                return (T) obj;
            else
                return null;
        }

        /// <summary>
        /// 获取对象的非公共方法
        /// </summary>
        /// <param name="type">对象类型</param>
        /// <param name="methodName">方法名</param>
        /// <returns></returns>
        public static MethodInfo GetObjectNoPublicMethod(Type type, string methodName)
        {
            return type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        /// 获取对象的非公共字段(变量)
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static FieldInfo GetObjectNoPublicField(Type type, string fieldName)
        {
            return type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        }


        public static string floatToPCT(float val, int places = 0)
        {
            string format = string.Concat("{", $"0:P{places}", "}");
            return string.Format(format, val); // "45.67%"
        }

        public static void Quit()
        {
            // 在编辑器中停止播放模式
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            UnityEngine.Application.Quit(); // 在独立平台退出应用
#endif
        }

        public static long UnixTimestampSeconds()
        {
            // 获取当前时间的 Unix 时间戳（秒）
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }
}