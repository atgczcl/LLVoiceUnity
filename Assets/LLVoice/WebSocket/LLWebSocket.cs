using LLVoice.Tools;
using LLVoice.Voice;
using System;
using UnityEngine;

namespace LLVoice.Net
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
        /// websocket连接成功回调
        /// </summary>
        public Action OnConnectCallback;

        public ILLWebSocket webSocket;

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
        public void Connect(Action onConnect = null, Action<string> onStrMsg = null, Action<byte[]> onByteMsg = null)
        {
            OnConnectCallback = onConnect;
            
            Uri uri = new(url);
#if UNITY_WEBGL
            //websocket连接
            webSocket = new LLWebSocketWebGL(uri);
#else
            //非webgl 平台 websocket连接
            webSocket = new LLWebSocketWindows(uri);
#endif
            webSocket.SetCallBack(onStrMsg, onByteMsg);
            StartCoroutine(webSocket.Connect(OnConnect));
        }

        /// <summary>
        /// websocket连接成功回调
        /// <summary>
        public void OnConnect()
        {
            Debug.Log("websocket连接成功");
            OnConnectCallback?.Invoke();
        }

        private void Update()
        {
            if (webSocket != null) webSocket.Update();
        }

        /// <summary>
        /// websocket发送消息 bytes
        /// </summary>
        /// <param name="data">字节</param>
        public void Send(byte[] data)
        {
            webSocket.Send(data);
        }

        /// <summary>
        /// websocket发送消息string
        /// </summary>
        /// <param name="msg">字符串</param>
        public void Send(string msg)
        {
            webSocket.Send(msg);
        }

        public void Close()
        {
            webSocket.Close();
        }


        private void OnDestroy()
        {
            Close();
        }
    }
    
}