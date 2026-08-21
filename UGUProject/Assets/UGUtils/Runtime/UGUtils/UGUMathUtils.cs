using UnityEngine;

namespace UGU.Runtime
{
    public static class UGUMathUtils
    {
        /// <summary>
        /// 将浮点数格式化为百分比字符串
        /// </summary>
        /// <param name="value">要格式化的值 (如 0.4567)</param>
        /// <param name="decimalPlaces">保留小数位数</param>
        /// <returns>百分比字符串 (如 "45.67%")</returns>
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
