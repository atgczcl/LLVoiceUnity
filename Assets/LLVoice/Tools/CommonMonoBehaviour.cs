using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace LLVoice.Tools
{
    /// <summary>
    /// 通用MonoBehaviour, 提供在主线程上执行操作的方法
    /// </summary>
    public class CommonMonoBehaviour : MonoBehaviour
    {
        public SynchronizationContext context { get; set; }

        public virtual void Awake()
        {
            context = SynchronizationContext.Current;
        }

        /// <summary>
        /// 在主线程上执行操作
        /// </summary>
        /// <param name="action"></param>
        public void InvokeOnMainThread(System.Action action)
        {
            // 回到主线程
            context.Post(_ =>
            {
                Debug.Log("回到主线程");
                action?.Invoke();
            }, null);
        }

        /// <summary>
        /// 在主线程上执行协程
        /// </summary>
        public void InvokeOnMainThread(IEnumerator enumerator)
        {
            // 回到主线程
            context.Post(_ =>
            {
                Debug.Log("回到主线程");
                StartCoroutine(enumerator);
            }, null);
        }
    }
}