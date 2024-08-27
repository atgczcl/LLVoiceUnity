using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LLVoice.Tools
{
    /// <summary>
    /// µ¥Àý½Å±¾
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
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
            //if (SceneManager.)
            DontDestroyOnLoad(gameObject);
            if (!instance)
            {
                if (!gameObject.TryGetComponent<T>(out instance)) instance = gameObject.AddComponent<T>();
            }
        }
    }

}
