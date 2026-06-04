using UnityEngine;

namespace UGU.Runtime.Utils
{
    /// <summary>
    /// 3D坐标点工具
    /// </summary>
    public static class UGUtilsCoordPointV3
    {
        /// <summary>
        /// 获取鼠标点的世界位置
        /// </summary>
        /// <param name="referenceCamera">参考相机</param>
        /// <param name="mouseZVal">鼠标点的深度，具体值根据你的场景而定</param>
        /// <returns></returns>
        public static Vector3 MousePointScreenToWorld(Camera referenceCamera, float mouseZVal)
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = mouseZVal; // 深度，具体值根据你的场景而定
            Vector3 worldPosition = referenceCamera.ScreenToWorldPoint(mousePosition);
            return worldPosition;
        }

        /// <summary>
        /// 计算射线与平面的交点
        /// </summary>
        /// <param name="rayOrigin">射线起点</param>
        /// <param name="rayDirection">射线方向（单位向量）</param>
        /// <param name="planePoint">平面上的一个点</param>
        /// <param name="planeNormal">平面的法向量（单位向量）</param>
        /// <returns>交点坐标。如果没有交点，返回 Vector3.positiveInfinity</returns>
        public static Vector3 GetRayPlaneIntersection(Vector3 rayOrigin, Vector3 rayDirection, Vector3 planePoint,
            Vector3 planeNormal)
        {
            float denominator = Vector3.Dot(planeNormal, rayDirection);

            // 如果分母为 0，说明射线与平面平行，没有交点
            if (Mathf.Abs(denominator) < 0.0001f) // 避免浮点数精度问题
            {
                return Vector3.positiveInfinity;
            }

            // 计算参数 t
            float t = Vector3.Dot(planeNormal, planePoint - rayOrigin) / denominator;

            // 如果 t < 0，说明交点在射线的反方向，没有有效交点
            if (t < 0)
            {
                return Vector3.positiveInfinity;
            }

            // 计算交点坐标
            return rayOrigin + t * rayDirection;
        }
    }
}