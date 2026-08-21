using UnityEngine;

namespace UGU.Runtime
{
    public static class UGUPropertyUtils
    {
        /// <summary>
        /// int 值大于等于 0
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int IntValueGreaterOrEqualZero(int value)
        {
            return Mathf.Clamp(value, 0, int.MaxValue);
        }
    }
}