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
    public class MonoSingleton<T> : CommonMonoBehaviour where T : CommonMonoBehaviour
    {
        // 获取当前的SynchronizationContext

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


        public override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
            if (!instance)
            {
                if (!gameObject.TryGetComponent<T>(out instance)) instance = gameObject.AddComponent<T>();
            }
        }
    }

}
