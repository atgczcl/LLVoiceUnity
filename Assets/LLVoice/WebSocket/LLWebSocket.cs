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
        public void Connect(string url, Action onConnect = null, Action<string> onStrMsg = null, Action<byte[]> onByteMsg = null)
        {
            OnConnectCallback = onConnect;
            this.url = url;
            Uri uri = new(url);
#if UNITY_WEBGL
            //websocket����
            webSocket = new LLWebSocketWebGL(uri);
#else
            //��webgl ƽ̨ websocket����
            webSocket = new LLWebSocketWindows(uri);
#endif
            webSocket.SetCallBack(onStrMsg, onByteMsg);
            StartCoroutine(webSocket.Connect(OnConnect));
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