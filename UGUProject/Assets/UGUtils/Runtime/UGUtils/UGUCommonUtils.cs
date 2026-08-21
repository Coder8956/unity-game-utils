using System;
using System.Reflection;
using UnityEngine;

namespace UGU.Runtime
{
    public static class UGUCommonUtils
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
        /// <param name="type">对象类型</param>
        /// <param name="fieldName">字段名</param>
        /// <returns></returns>
        public static FieldInfo GetObjectNoPublicField(Type type, string fieldName)
        {
            return type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        /// 将浮点数转换为百分比字符串
        /// </summary>
        /// <param name="val">要格式化的值</param>
        /// <param name="places">保留小数位数</param>
        /// <returns>百分比字符串</returns>
        public static string FloatToPct(float val, int places = 0)
        {
            return val.ToString($"P{places}");
        }

        /// <summary>
        /// 退出应用
        /// </summary>
        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// 获取当前时间的 Unix 时间戳（秒）
        /// </summary>
        /// <returns>Unix 时间戳（秒）</returns>
        public static long UnixTimestampSeconds()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }
}
