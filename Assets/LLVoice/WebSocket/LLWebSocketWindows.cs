using System;
using WebSocketSharp;
using UnityEngine;
using UnityEditor.PackageManager;
using System.Text;

namespace LLStar.Net
{
    ///<summary>
    ///windows平台websocket连接
    ///</summary>
    public class LLWebSocketWindows
    {
        ///<summary>
        ///websocket客户端
        ///</summary>
        public static WebSocket client;

        ///<summary>
        ///收到数据回调
        public static Action<string> ReceiveCallback;
        public static Action OnConnect;

        ///<summary>
        ///websocket连接
        ///</summary>
        /// <param name="url">websocket地址</param>
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
            //忽略证书验证
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
        ///发送数据
        ///</summary>
        ///<param name="data">数据</param>
        public static void Send(byte[] data)
        {
            if (client != null && client.IsAlive)
                client.Send(data);
        }

        ///<summary>
        ///发送数据 string
        ///</summary>
        ///<param name="data">数据</param>
        public static void Send(string data)
        {
            // 将字符串转换为UTF-8编码的字节数组
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(data);
            // 将字节数组转换回字符串
            string utf8String = Encoding.UTF8.GetString(utf8Bytes);
            if (client != null && client.IsAlive)
                client.Send(utf8String);
        }

        ///<summary>
        ///关闭连接
        ///</summary>
        public static void Close()
        {
            client.Close(CloseStatusCode.Normal, "Closing connection");
            client = null;
        }
    }
}