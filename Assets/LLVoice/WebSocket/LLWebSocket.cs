using LLVoice.Tools;
using LLVoice.Voice;
using System;
using UnityEngine;

namespace LLStar.Net
{
    /// <summary>
    /// websocket ��װ�������˲�ͬƽ̨����, �����˰�ȫ�Լ��
    /// </summary>
    public class LLWebSocket : MonoSingleton<LLWebSocket>
    {
        ///<summary>
        ///websocket��ַ
        ///</summary>
        public string url = "wss://127.0.0.1:10096/";
        /// <summary>
        /// websocket�յ���Ϣ�ص�
        /// </summary>
        public Action<string> OnMessageCallback;
        /// <summary>
        /// websocket���ӳɹ��ص�
        /// </summary>
        public Action OnConnectCallback;

        //private void Start()
        //{
        //    Debug.Log("websocket start test");
        //    Connect( onConnect: () => {
        //        LLVoiceManager.Instance.Initialized();
        //    });
        //}

        ///<summary>
        ///����websocket
        ///</summary>
        public void Connect(Action<string> onMessageCallback = null, Action onConnect = null)
        {
            OnMessageCallback = onMessageCallback;
            OnConnectCallback = onConnect;
#if UNITY_WEBGL
            //websocket����
#else
            //��webgl ƽ̨ websocket����
            LLWebSocketWindows.Connect(url, OnMessage, OnConnect);
#endif
        }

        /// <summary>
        /// websocket�յ���Ϣ
        /// </summary>
        public void OnMessage(string msg)
        {
            //Debug.Log("websocket�յ���Ϣ:" + msg);
            OnMessageCallback?.Invoke(msg);
        }

        /// <summary>
        /// websocket���ӳɹ��ص�
        /// <summary>
        public void OnConnect()
        {
            Debug.Log("websocket���ӳɹ�");
            OnConnectCallback?.Invoke();
        }

        /// <summary>
        /// websocket������Ϣ bytes
        /// </summary>
        /// <param name="data">�ֽ�</param>
        public void Send(byte[] data)
        {
            //Debug.Log("������Ϣ:" + data.Length);
            #if UNITY_WEBGL
            //websocket������Ϣ
            #else
            //��webgl ƽ̨ websocket������Ϣ
            LLWebSocketWindows.Send(data);
            #endif
        }

        /// <summary>
        /// websocket������Ϣstring
        /// </summary>
        /// <param name="msg">�ַ���</param>
        public void Send(string msg)
        {
            #if UNITY_WEBGL
            //websocket������Ϣ
            #else
            //��webgl ƽ̨ websocket������Ϣ
            LLWebSocketWindows.Send(msg);
            #endif
        }

        public void Close()
        {
            #if UNITY_WEBGL
            //websocket�ر�
            #else
            //��webgl ƽ̨ websocket�ر�
            LLWebSocketWindows.Close();
            #endif
        }


        private void OnDestroy()
        {
            Close();
        }
    }
    
}