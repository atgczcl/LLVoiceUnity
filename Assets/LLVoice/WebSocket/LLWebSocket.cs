using LLVoice.Tools;
using LLVoice.Voice;
using System;
using UnityEngine;

namespace LLStar.Net
{
    /// <summary>
    /// websocket 封装，屏蔽了不同平台差异, 屏蔽了安全性检查
    /// </summary>
    public class LLWebSocket : MonoSingleton<LLWebSocket>
    {
        ///<summary>
        ///websocket地址
        ///</summary>
        public string url = "wss://127.0.0.1:10096/";
        /// <summary>
        /// websocket收到消息回调
        /// </summary>
        public Action<string> OnMessageCallback;
        /// <summary>
        /// websocket连接成功回调
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
        ///连接websocket
        ///</summary>
        public void Connect(Action<string> onMessageCallback = null, Action onConnect = null)
        {
            OnMessageCallback = onMessageCallback;
            OnConnectCallback = onConnect;
#if UNITY_WEBGL
            //websocket连接
#else
            //非webgl 平台 websocket连接
            LLWebSocketWindows.Connect(url, OnMessage, OnConnect);
#endif
        }

        /// <summary>
        /// websocket收到消息
        /// </summary>
        public void OnMessage(string msg)
        {
            //Debug.Log("websocket收到消息:" + msg);
            OnMessageCallback?.Invoke(msg);
        }

        /// <summary>
        /// websocket连接成功回调
        /// <summary>
        public void OnConnect()
        {
            Debug.Log("websocket连接成功");
            OnConnectCallback?.Invoke();
        }

        /// <summary>
        /// websocket发送消息 bytes
        /// </summary>
        /// <param name="data">字节</param>
        public void Send(byte[] data)
        {
            //Debug.Log("发送消息:" + data.Length);
            #if UNITY_WEBGL
            //websocket发送消息
            #else
            //非webgl 平台 websocket发送消息
            LLWebSocketWindows.Send(data);
            #endif
        }

        /// <summary>
        /// websocket发送消息string
        /// </summary>
        /// <param name="msg">字符串</param>
        public void Send(string msg)
        {
            #if UNITY_WEBGL
            //websocket发送消息
            #else
            //非webgl 平台 websocket发送消息
            LLWebSocketWindows.Send(msg);
            #endif
        }

        public void Close()
        {
            #if UNITY_WEBGL
            //websocket关闭
            #else
            //非webgl 平台 websocket关闭
            LLWebSocketWindows.Close();
            #endif
        }


        private void OnDestroy()
        {
            Close();
        }
    }
    
}