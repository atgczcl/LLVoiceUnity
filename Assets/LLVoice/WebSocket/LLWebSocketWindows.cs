using System;
using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using WebSocketSharp;

namespace LLVoice.Net
{
    ///<summary>
    ///windows平台websocket连接
    ///</summary>
    public class LLWebSocketWindows: ILLWebSocket
    {
        private Uri mUrl;
        WebSocketSharp.WebSocket m_Socket;
        Queue<MessageEventArgs> m_Messages = new Queue<MessageEventArgs>();
        bool m_IsConnected = false;
        string m_Error = null;
        /// <summary>
        /// websocket收到消息回调
        /// </summary>
        public Action<string> OnStringMessageCallback;
        public Action<byte[]> OnByteMessageCallback;

        public LLWebSocketWindows(Uri url)
        {
            mUrl = url;

            string protocol = mUrl.Scheme;
            if (!protocol.Equals("ws") && !protocol.Equals("wss"))
                throw new ArgumentException("Unsupported protocol: " + protocol);
        }

        public void Send(string str)
        {
            m_Socket.Send(str);
        }

        public IEnumerator Connect(Action onConnect = null)
        {
            m_Socket = new WebSocketSharp.WebSocket(mUrl.ToString());
            m_Socket.OnMessage += (sender, e) => OnWebSocketMessage(e);
            m_Socket.OnOpen += (sender, e) => { 
                m_IsConnected = true; 
                onConnect?.Invoke();
            };
            m_Socket.OnError += (sender, e) => m_Error = e.Message;
            m_Socket.SslConfiguration.ServerCertificateValidationCallback =
                  (sender, certificate, chain, sslPolicyErrors) =>
                  {
                      // Do something to validate the server certificate.
                      //...

                      return true; // If the server certificate is valid.
                  };
            m_Socket.ConnectAsync();
            while (!m_IsConnected && m_Error == null)
                yield return 0;
            
        }

        private void OnWebSocketMessage(MessageEventArgs e)
        {
            Debug.Log("OnWebSocketMessage: " + e.Data);
            m_Messages.Enqueue(e);
        }

        public void OnMessage(byte[] buffer)
        {
            OnByteMessageCallback?.Invoke(buffer);
        }

        public void OnMessage(string str)
        {
            OnStringMessageCallback?.Invoke(str);
        }

        public void Update()
        {
            while (m_Messages.Count > 0)
            {
                MessageEventArgs e = m_Messages.Dequeue();
                OnMessage(e.Data);
                OnMessage(e.RawData);
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="buffer"></param>

        public void Send(byte[] buffer)
        {
            m_Socket.Send(buffer);
        }

        public MessageEventArgs Recv()
        {
            if (m_Messages.Count == 0)
                return null;
            return m_Messages.Dequeue();
        }

        public string RecvString()
        {
            MessageEventArgs e = Recv();
            if (e == null)
                return null;
            return e.Data;
        }

        public byte[] RecvByte()
        {
            MessageEventArgs msg = Recv();
            if (msg == null)
                return null;
            return msg.RawData;
        }

        public void Close()
        {
            m_Socket.Close();
        }

        public void SetCallBack(Action<string> OnStringMessageCallback, Action<byte[]> OnByteMessageCallback)
        {
            this.OnStringMessageCallback = OnStringMessageCallback;
            this.OnByteMessageCallback = OnByteMessageCallback;
        }

        public string error
        {
            get
            {
                return m_Error;
            }
        }
    }

    ///<summary>
    ///LLWebSocket for Windows和WebGL的公共接口，统一函数名
    ///</summary>
    public interface ILLWebSocket { 
        public void Send(string str);
        public void Send(byte[] buffer);
        public IEnumerator Connect(Action onConnect = null);
        public void SetCallBack(Action<string> OnStringMessageCallback, Action<byte[]> OnByteMessageCallback);

        public void OnMessage(string str);
        public void OnMessage(byte[] buffer);
        public void Update();

        public byte[] RecvByte();
        public string RecvString();
        public void Close();
    }
}