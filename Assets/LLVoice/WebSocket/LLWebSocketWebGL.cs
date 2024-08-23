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

        public LLWebSocketWebGL(Uri url)
        {
            mUrl = url;

            string protocol = mUrl.Scheme;
            if (!protocol.Equals("ws") && !protocol.Equals("wss"))
                throw new ArgumentException("Unsupported protocol: " + protocol);
        }

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

//#if UNITY_WEBGL && !UNITY_EDITOR
		[DllImport("__Internal")]
		private static extern int SocketCreate (string url);

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

		public IEnumerator Connect(Action onConnect = null)
		{
			m_NativeRef = SocketCreate (mUrl.ToString());

			while (SocketState(m_NativeRef) == 0)
				yield return 0;
            onConnect?.Invoke();
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
            if (SocketState(m_NativeRef) == 1)
            {
                byte[] retval = RecvByte();
                if (retval == null)
                    return;
                OnMessage(Encoding.UTF8.GetString(retval));
                OnMessage(retval);
            }
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
//#endif
	}

}