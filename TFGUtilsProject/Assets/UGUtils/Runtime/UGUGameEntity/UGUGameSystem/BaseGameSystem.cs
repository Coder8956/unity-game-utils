using UnityEngine;

namespace UGUtils.Runtime.UGUGameEntity
{
    public abstract class BaseGameSystem : MonoBehaviour
    {
        /// <summary>
        /// 最先调用
        /// </summary>
        protected abstract void OnCreate();

        public virtual void Init(object arg = null)
        {
        }

        public virtual void AppExit(object arg = null)
        {
            Debug.Log($"App Exit: {GetType().Name}");
        }
    }
}