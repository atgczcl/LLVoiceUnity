using LLVoice.Tools;
using LLVoice.Voice;
using System;
using UnityEngine;

namespace LLVoice.Net
{
    /// <summary>
    /// websocket ��װ�������˲�ͬƽ̨����, �����˰�ȫ�Լ��
    /// </summary>
    public class LLWebSocket : CommonMonoBehaviour
    {
        ///<summary>
        ///websocket��ַ
        ///</summary>
        public string url = "wss://127.0.0.1:10096/";

        /// <summary>
        /// websocket���ӳɹ��ص�
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
        ///����websocket
        ///</summary>
        /// <param name="url"> websocket url:ws://192.168.1.1:8080, wss://192.168.1.1:8080</param>
        /// <param name="onConnect">���ӳɹ��ص���ע��Ĭ���Ѿ��������л����̵߳��ã����Ե���unity���̷߳���</param>
        /// <param name="onStrMsg">�յ��ַ�����Ϣ�ص�, ͨ��Update���͹����������߳�</param>
        /// <param name="onByteMsg">�յ��ֽ���Ϣ�ص�, ͨ��Update���͹����������߳�</param>
        public void Connect(string url, Action onConnect = null, Action<string> onStrMsg = null, Action<byte[]> onByteMsg = null, Action<string> OnCloseCall = null)
        {
            OnConnectCallback = onConnect;
            OnCloseCallback = OnCloseCall;
            this.url = url;
            Uri uri = new(url);
#if UNITY_WEBGL
            //websocket����
            webSocket = new LLWebSocketWebGL(uri, gameObject.name);
#else
            //��webgl ƽ̨ websocket����
            webSocket = new LLWebSocketWindows(uri);
#endif
            webSocket.SetCallBack(onStrMsg, onByteMsg);
            StartCoroutine(webSocket.Connect(OnConnect, OnClose));
        }

        /// <summary>
        /// websocket���ӳɹ��ص�
        /// <summary>
        public void OnConnect()
        {
            Debug.Log("websocket���ӳɹ�");
            //���ӳɹ����첽�߳��У���Ҫ�л������̵߳��ã����ܱ����޷�����unity����
            //����Ƶ�����÷���������Ҫ�л������̣߳���Ϊunity���������̰߳�ȫ�ģ�����Ƶ���л��̴߳�������������
            InvokeOnMainThread(() => { 
                OnConnectCallback?.Invoke();
            });
        }

        private void Update()
        {
            webSocket?.Update();
        }

        /// <summary>
        /// websocket������Ϣ bytes
        /// </summary>
        /// <param name="data">�ֽ�</param>
        public void Send(byte[] data)
        {
            webSocket.Send(data);
        }

        /// <summary>
        /// websocket������Ϣstring
        /// </summary>
        /// <param name="msg">�ַ���</param>
        public void Send(string msg)
        {
            webSocket.Send(msg);
        }

        /// <summary>
        /// OnClose
        /// </summary>
        public void OnClose(string errorMsg)
        {
            Debug.LogError("websocket���ӹر�:" + errorMsg);
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