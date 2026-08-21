using System;
using System.Reflection;

namespace UGU.Editor
{
    public class EUGUCommonUtils
    {
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
    }
}