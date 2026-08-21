using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UGU.Runtime
{
    public static class UGUtilMath
    {
        public static string ToPercent(float value, int decimalPlaces = 2)
        {
            return value.ToString("P" + decimalPlaces);
        }

        /// <summary>
        /// 根据概率随机判断是否可以执行某事件
        /// </summary>
        /// <param name="probability">概率 [0, 1]，如 0.7 表示 70%</param>
        /// <returns>是否可以执行</returns>
        public static bool IsProbabilityMet(float probability)
        {
            return UnityEngine.Random.value <= probability;
        }
    }
}