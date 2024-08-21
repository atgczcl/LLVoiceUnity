using System;
using WebSocketSharp;
using UnityEngine;
using UnityEditor.PackageManager;
using System.Text;

namespace LLStar.Net
{
    ///<summary>
    ///windowsƽ̨websocket����
    ///</summary>
    public class LLWebSocketWindows
    {
        ///<summary>
        ///websocket�ͻ���
        ///</summary>
        public static WebSocket client;

        ///<summary>
        ///�յ����ݻص�
        public static Action<string> ReceiveCallback;
        public static Action OnConnect;

        ///<summary>
        ///websocket����
        ///</summary>
        /// <param name="url">websocket��ַ</param>
        public static void Connect(string url, Action<string> callback = null, Action onConnect = null)
        {
            ReceiveCallback = callback;
            OnConnect = onConnect;
            
            client = new WebSocket(url);
            client.OnOpen += OnWebSocketOpen;
            client.OnMessage += OnWebSocketMessage;
            client.OnError += OnWebSocketError;
            client.OnClose += OnWebSocketClose;
            client.Log.Output += OnWebSocketLog;
            client.Log.Level = WebSocketSharp.LogLevel.Debug;
            client.SslConfiguration.ServerCertificateValidationCallback =
              (sender, certificate, chain, sslPolicyErrors) => {
                  // Do something to validate the server certificate.
                  //...

                return true; // If the server certificate is valid.
            };
            //����֤����֤
            //client.Options.SetRequestHeader("");
            client.Connect();
        }

        public static void OnWebSocketLog(LogData data, string arg2)
        {
            Debug.Log($"WebSocketLog: {data.Message}");
        }

        public static void OnWebSocketOpen(object sender, EventArgs e)
        {
            Debug.Log("WebSocket opened");
            OnConnect?.Invoke();
        }

        public static void OnWebSocketMessage(object sender, MessageEventArgs e)
        {
            //Debug.Log("WebSocket message: " + e.Data);
            ReceiveCallback?.Invoke(e.Data);
        }

        public static void OnWebSocketError(object sender, ErrorEventArgs e)
        {
            Debug.Log("WebSocket error: " + e.Message);
        }

        public static void OnWebSocketClose(object sender, CloseEventArgs e)
        {
            Debug.Log($"WebSocket closed: {e.Reason}|{e.Code}|{e.WasClean}");
        }


        ///<summary>
        ///��������
        ///</summary>
        ///<param name="data">����</param>
        public static void Send(byte[] data)
        {
            if (client != null && client.IsAlive)
                client.Send(data);
        }

        ///<summary>
        ///�������� string
        ///</summary>
        ///<param name="data">����</param>
        public static void Send(string data)
        {
            // ���ַ���ת��ΪUTF-8������ֽ�����
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(data);
            // ���ֽ�����ת�����ַ���
            string utf8String = Encoding.UTF8.GetString(utf8Bytes);
            if (client != null && client.IsAlive)
                client.Send(utf8String);
        }

        ///<summary>
        ///�ر�����
        ///</summary>
        public static void Close()
        {
            client.Close(CloseStatusCode.Normal, "Closing connection");
            client = null;
        }
    }
}