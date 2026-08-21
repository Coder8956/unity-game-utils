using System;
using System.Collections.Generic;
using UnityEngine;

namespace UGU.Runtime
{
    public class UGUtilGO
    {
        public static T CreateGameObject<T>(string name, GameObject parent) where T : Component
        {
            GameObject GO = new GameObject(name, typeof(T));
            if (parent)
            {
                GO.transform.SetParent(parent.transform);
            }

            GO.transform.localPosition = Vector3.zero;
            // Debug.Log($"The {name} is created");
            return GO.GetComponent<T>();
        }

        /// <summary>
        /// 清理子物体
        /// </summary>
        /// <param name="go"></param>
        public static void ClearChildGO(GameObject go)
        {
            List<GameObject> ChildObjects = new List<GameObject>();
            foreach (Transform t in go.transform)
            {
                ChildObjects.Add(t.gameObject);
            }

            foreach (var ChildGO in ChildObjects)
            {
                GameObject.Destroy(ChildGO);
            }
        }

        /// <summary>
        /// 创建子物体
        /// </summary>
        /// <param name="parent">父物体</param>
        /// <param name="ChildGO">子物体</param>
        /// <param name="count">子物体数量</param>
        // public static List<GameObject> CreateChildGO(GameObject parent, GameObject ChildGO, int count)
        // {
        //     List<GameObject> ChildObjects = new List<GameObject>();
        //     for (int i = 0; i < count; i++)
        //     {
        //         GameObject newChildGO = GameObject.Instantiate(ChildGO, parent.transform, false);
        //         ChildObjects.Add(newChildGO);
        //     }
        //
        //     return ChildObjects;
        // }
        public static List<T> UpdateActiveChilds<T>(GameObject parent, GameObject childTemplate, int count,
            bool destroyRedundant = false) where T : MonoBehaviour
        {
            // 获取已经存在的子物体
            T[] Childs = parent.GetComponentsInChildren<T>(true);
            List<T> existentChilds = new List<T>(Childs);

            // 移除模板对象
            existentChilds.Remove(childTemplate.GetComponent<T>());

            // 失活已经存在的子物体&模板物体
            for (var i = 0; i < Childs.Length; i++)
            {
                if (Childs[i].gameObject.activeSelf)
                {
                    Childs[i].gameObject.SetActive(false);
                }
            }

            // 激活的子物体
            List<T> activeChilds = new List<T>();

            if (existentChilds.Count >= count)
            {
                // 处理 已有的子物体大于等于需要的子物体
                for (int i = 0; i < count; i++)
                {
                    existentChilds[i].gameObject.SetActive(true);
                    activeChilds.Add(existentChilds[i]);
                }
            }
            else
            {
                // 处理 已有的子物体小于需要的子物体
                // 激活已有的子物体
                for (var i = 0; i < existentChilds.Count; i++)
                {
                    existentChilds[i].gameObject.SetActive(true);
                }

                activeChilds.AddRange(existentChilds);

                // 计算需要创建的子物体
                int createNum = count - existentChilds.Count;

                for (int i = 0; i < createNum; i++)
                {
                    GameObject newChild = GameObject.Instantiate(childTemplate.gameObject, parent.transform, false);
                    newChild.SetActive(true);
                    activeChilds.Add(newChild.GetComponent<T>());
                }
            }

            return activeChilds;
        }
    }
}