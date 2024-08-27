using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace LLVoice.Tools
{
    /// <summary>
    /// 单例脚本
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        // 获取当前的SynchronizationContext
        public SynchronizationContext context;

        private static T instance;

        public static T Instance
        {
            get
            {
                if (!instance)
                {
                    instance = FindObjectOfType<T>(true);
                    if (instance == null)
                    {
                        GameObject gameObject = GameObject.Find(typeof(T).Name);
                        if (gameObject == null)
                        {
                            gameObject = new GameObject(typeof(T).Name);
                        }
                        instance = gameObject.GetComponent<T>();
                        if (instance == null) instance = gameObject.AddComponent<T>();
                    }
                }
                return instance;
            }
            set { instance = value; }
        }


        public virtual void Awake()
        {
            context = SynchronizationContext.Current;
            DontDestroyOnLoad(gameObject);
            if (!instance)
            {
                if (!gameObject.TryGetComponent<T>(out instance)) instance = gameObject.AddComponent<T>();
            }
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
