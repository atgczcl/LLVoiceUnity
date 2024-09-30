using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace LLVoice.Net
{
    public class LLWebSocketWebGL: ILLWebSocket
    {
        private Uri mUrl;
        /// <summary>
        /// websocket收到消息回调
        /// </summary>
        public Action<string> OnStringMessageCallback;
        public Action<byte[]> OnByteMessageCallback;
        public string objName;

        public LLWebSocketWebGL(Uri url, string objName)
        {
            mUrl = url;
            this.objName = objName;
            string protocol = mUrl.Scheme;
            if (!protocol.Equals("ws") && !protocol.Equals("wss"))
                throw new ArgumentException("Unsupported protocol: " + protocol);
        }


#if UNITY_WEBGL && !UNITY_EDITOR
		[DllImport("__Internal")]
		private static extern int SocketCreate (string url, string objName);

		[DllImport("__Internal")]
		private static extern int SocketState (int socketInstance);

		[DllImport("__Internal")]
		private static extern void SocketSend (int socketInstance, byte[] ptr, int length);

        [DllImport("__Internal")]
        private static extern void SocketSendString(int socketInstance, string str);

        [DllImport("__Internal")]
		private static extern void SocketRecv (int socketInstance, byte[] ptr, int length);

		[DllImport("__Internal")]
		private static extern int SocketRecvLength (int socketInstance);

		[DllImport("__Internal")]
		private static extern void SocketClose (int socketInstance);

		[DllImport("__Internal")]
		private static extern int SocketError (int socketInstance, byte[] ptr, int length);

        int m_NativeRef = 0;

        public void Send(string str)
        {
            //Send(Encoding.UTF8.GetBytes(str));
            SocketSendString(m_NativeRef, str);
        }

        public string RecvString()
        {
            byte[] retval = RecvByte();
            if (retval == null)
                return null;
            return Encoding.UTF8.GetString(retval);
        }

		public void Send(byte[] buffer)
		{
			SocketSend (m_NativeRef, buffer, buffer.Length);
		}

		public byte[] Recv()
		{
			int length = SocketRecvLength (m_NativeRef);
			if (length == 0)
				return null;
			byte[] buffer = new byte[length];
			SocketRecv (m_NativeRef, buffer, length);
			return buffer;
		}

		public IEnumerator Connect(Action onConnect = null, Action<string> OnCloseCallback = null)
		{
			m_NativeRef = SocketCreate (mUrl.ToString(), objName);

			while (SocketState(m_NativeRef) == 0)
				yield return 0;
            if (SocketState(m_NativeRef) == 1)
                onConnect?.Invoke();
        }

        public void OnClose(string errorMsg)
        {
            //Debug.LogError("websocket连接关闭:" + errorMsg);
            //OnCloseCallback?.Invoke(errorMsg);
        }
 
		public void Close()
		{
			SocketClose(m_NativeRef);
		}

        public void OnMessage(string str)
        {
            OnStringMessageCallback?.Invoke(str);
        }

        public void OnMessage(byte[] buffer)
        {
            OnByteMessageCallback?.Invoke(buffer);
        }

        public void Update()
        {
            //只在webgl上执行
            //#if UNITY_WEBGL && !UNITY_EDITOR
            if (SocketState(m_NativeRef) == 1)
            {
                byte[] retval = RecvByte();
                if (retval == null)
                    return;
                OnMessage(Encoding.UTF8.GetString(retval));
                OnMessage(retval);
            }
            //#endif
        }

        public byte[] RecvByte()
        {
            int length = SocketRecvLength(m_NativeRef);
            if (length == 0)
                return null;
            byte[] buffer = new byte[length];
            SocketRecv(m_NativeRef, buffer, length);
            return buffer;
        }

        public void SetCallBack(Action<string> OnStringMessageCallback, Action<byte[]> OnByteMessageCallback)
        {
            this.OnStringMessageCallback = OnStringMessageCallback;
            this.OnByteMessageCallback = OnByteMessageCallback;
        }

        public string error
		{
			get {
				const int bufsize = 1024;
				byte[] buffer = new byte[bufsize];
				int result = SocketError (m_NativeRef, buffer, bufsize);

				if (result == 0)
					return null;

				return Encoding.UTF8.GetString (buffer);				
			}
		}
#else
        public void Send(string str)
        {
        }

        public void Send(byte[] buffer)
        {
        }

        public IEnumerator Connect(Action onConnect = null, Action<string> OnCloseCallback = null)
        {
            Debug.LogError("Editor Run WebGLSocket is not support!");
            yield break;
        }

        public void SetCallBack(Action<string> OnStringMessageCallback, Action<byte[]> OnByteMessageCallback)
        {
            
        }

        public void OnMessage(string str)
        {
            
        }

        public void OnMessage(byte[] buffer)
        {
            
        }

        public void Update()
        {
            
        }

        public byte[] RecvByte()
        {
            return null;
        }

        public string RecvString()
        {
            return null;
        }

        public void OnClose(string errorMsg)
        {
            //Debug.LogError("websocket连接关闭:" + errorMsg);
            //OnCloseCallback?.Invoke(errorMsg);
        }

        public void Close()
        {
            
        }
#endif
    }

}