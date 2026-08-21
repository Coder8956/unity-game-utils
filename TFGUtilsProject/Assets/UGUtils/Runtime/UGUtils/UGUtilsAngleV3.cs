using UnityEngine;

namespace UGU.Runtime
{
    /// <summary>
    /// 向量工具
    /// </summary>
    public static class UGUtilsAngleV3
    {
        /// <summary>
        /// 计算两个向量之间的角度 (0 到 360 度)
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static float CalculateAngle360(Vector3 from, Vector3 to)
        {
            // 计算两个向量的点积
            float dot = Vector3.Dot(from.normalized, to.normalized);

            // 计算两个向量的叉积
            Vector3 cross = Vector3.Cross(from, to);

            // 计算角度 (以弧度为单位)
            float angleRad = Mathf.Acos(dot);

            // 将角度转换为度数
            float angleDeg = angleRad * Mathf.Rad2Deg;

            // 如果叉积的 z 分量为负，则角度为 360 度减去计算得到的角度
            if (cross.z < 0)
            {
                angleDeg = 360f - angleDeg;
            }

            return angleDeg;
        }

        /// <summary>
        /// 计算两个向量之间的角度 (0 到 180 度)
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static float CalculateAngle180(Vector3 from, Vector3 to)
        {
            // 计算两个向量的点积
            float dot = Vector3.Dot(from.normalized, to.normalized);

            // 使用反余弦函数计算夹角 (弧度)
            float angleRad = Mathf.Acos(dot);

            // 将角度转换为度数
            float angleDeg = angleRad * Mathf.Rad2Deg;

            return angleDeg;
        }

        /// <summary>
        /// 计算两个向量之间的角度 (-180 到 180 度)
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static float CalculateSignedAngle180(Vector3 from, Vector3 to)
        {
            // 计算两个向量的点积
            float dot = Vector3.Dot(from.normalized, to.normalized);

            // 使用反余弦函数计算夹角 (弧度)
            float angleRad = Mathf.Acos(dot);

            // 将角度转换为度数
            float angleDeg = angleRad * Mathf.Rad2Deg;

            // 计算两个向量的叉积
            Vector3 cross = Vector3.Cross(from, to);

            // 如果叉积的 z 分量为负，则角度取负值
            if (cross.z < 0)
            {
                angleDeg = -angleDeg;
            }

            return angleDeg;
        }

        // 根据给定的角度和旋转轴计算目标向量
        public static Vector3 CalculateTargetVectorByAngle(Vector3 from, float angle, Vector3 axis)
        {
            // 创建旋转四元数
            Quaternion rotation = Quaternion.AngleAxis(angle, axis);

            // 旋转向量
            Vector3 targetVector = rotation * from;

            return targetVector;
        }
    }
}