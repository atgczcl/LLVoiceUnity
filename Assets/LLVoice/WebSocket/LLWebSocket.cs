using LLVoice.Tools;
using LLVoice.Voice;
using System;
using UnityEngine;

namespace LLVoice.Net
{
    /// <summary>
    /// websocket 封装，屏蔽了不同平台差异, 屏蔽了安全性检查
    /// </summary>
    public class LLWebSocket : CommonMonoBehaviour
    {
        ///<summary>
        ///websocket地址
        ///</summary>
        public string url = "wss://127.0.0.1:10096/";

        /// <summary>
        /// websocket连接成功回调
        /// </summary>
        public Action OnConnectCallback;
        public Action<string> OnCloseCallback;

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
        /// <param name="url"> websocket url:ws://192.168.1.1:8080, wss://192.168.1.1:8080</param>
        /// <param name="onConnect">连接成功回调，注：默认已经采用了切回主线程调用，可以调用unity主线程方法</param>
        /// <param name="onStrMsg">收到字符串消息回调, 通过Update发送过来，在主线程</param>
        /// <param name="onByteMsg">收到字节消息回调, 通过Update发送过来，在主线程</param>
        public void Connect(string url, Action onConnect = null, Action<string> onStrMsg = null, Action<byte[]> onByteMsg = null, Action<string> OnCloseCall = null)
        {
            OnConnectCallback = onConnect;
            OnCloseCallback = OnCloseCall;
            this.url = url;
            Uri uri = new(url);
#if UNITY_WEBGL
            //websocket连接
            webSocket = new LLWebSocketWebGL(uri, gameObject.name);
#else
            //非webgl 平台 websocket连接
            webSocket = new LLWebSocketWindows(uri);
#endif
            webSocket.SetCallBack(onStrMsg, onByteMsg);
            StartCoroutine(webSocket.Connect(OnConnect, OnClose));
        }

        /// <summary>
        /// websocket连接成功回调
        /// <summary>
        public void OnConnect()
        {
            Debug.Log("websocket连接成功");
            //连接成功在异步线程中，需要切换到主线程调用，才能避免无法调用unity方法
            //其他频繁调用方法，不需要切换到主线程，因为unity方法都是线程安全的，避免频繁切换线程带来的性能消耗
            InvokeOnMainThread(() => { 
                OnConnectCallback?.Invoke();
            });
        }

        private void Update()
        {
            webSocket?.Update();
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

        /// <summary>
        /// OnClose
        /// </summary>
        public void OnClose(string errorMsg)
        {
            Debug.LogError("websocket连接关闭:" + errorMsg);
            OnCloseCallback?.Invoke(errorMsg);
        }

        public void Close()
        {
            webSocket?.Close();
        }


        private void OnDestroy()
        {
            Close();
        }
    }
    
}