using UnityEngine;

namespace UGU.Runtime
{
    /// <summary>
    /// 输入工具
    /// </summary>
    public static class UGUtilsInput
    {
        /// <summary>
        /// 判断鼠标是否在屏幕内
        /// </summary>
        /// <param name="mousePosition">鼠标的屏幕坐标</param>
        /// <returns>是否在屏幕内</returns>
        public static bool IsMouseInScreen()
        {
            Vector3 mousePosition = Input.mousePosition;

            // 检查鼠标的 x 和 y 坐标是否在屏幕范围内
            bool isXInScreen = mousePosition.x >= 0 && mousePosition.x <= Screen.width;
            bool isYInScreen = mousePosition.y >= 0 && mousePosition.y <= Screen.height;

            // 如果 x 和 y 都在屏幕范围内，则鼠标在屏幕内
            return isXInScreen && isYInScreen;
        }
    }
}