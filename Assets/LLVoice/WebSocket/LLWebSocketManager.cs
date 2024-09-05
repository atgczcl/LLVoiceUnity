using LLVoice.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LLVoice.Net
{
    /// <summary>
    /// WebSocket管理器
    /// </summary>
    public class LLWebSocketManager : MonoSingleton<LLWebSocketManager>
    {
        public readonly Dictionary<string, LLWebSocket> webSockets = new();

        /// <summary>
        /// 添加WebSocket
        /// </summary>
        /// <param name="key">websocket ID key</param>
        /// <param name="url"> websocket url:ws://192.168.1.1:8080, wss://192.168.1.1:8080</param>
        /// <param name="onConnect">连接成功回调，注：默认已经采用了切回主线程调用，可以调用unity主线程方法</param>
        /// <param name="onStrMsg">收到字符串消息回调, 通过Update发送过来，在主线程</param>
        /// <param name="onByteMsg">收到字节消息回调, 通过Update发送过来，在主线程</param>
        public LLWebSocket AddWebSocket(string key, string url, Action onConnect = null, Action<string> onStrMsg = null, Action<byte[]> onByteMsg = null)
        {
            if (!webSockets.ContainsKey(key)) { 
                var ws = new GameObject($"webSocket_{key}").AddComponent<LLWebSocket>();
                ws.transform.SetParent(transform);
                ws.Connect(url,onConnect, onStrMsg, onByteMsg);
                webSockets.Add(key, ws);
                return ws;
            }
            else
            {
                Debug.LogError("WebSocket already exists with key: " + key);
                return null;
            }
        }

        public void RemoveWebSocket(string key)
        {
            if (webSockets.ContainsKey(key))
            {
                webSockets[key].Close();
                Destroy(webSockets[key].gameObject);
                webSockets.Remove(key);
            }
            else
            {
                Debug.LogError("WebSocket not found with key: " + key);
            }
        }

        /// <summary>
        /// 发送消息string
        /// </summary>
        /// <param name="key"></param>
        /// <param name="message"></param>
        public void Send(string key, string message)
        {
            if (webSockets.ContainsKey(key))
            {
                webSockets[key].Send(message);
            }
            else
            {
                Debug.LogError("WebSocket not found with key: " + key);
            }
        }

        /// <summary>
        /// 发送消息byte[]
        /// </summary>
        /// <param name="key"></param>
        /// <param name="message"></param>
        public void Send(string key, byte[] message)
        {
            if (webSockets.ContainsKey(key))
            {
                webSockets[key].Send(message);
            }
            else
            {
                Debug.LogError("WebSocket not found with key: " + key);
            }
        }

        // 新增获取 WebSocket 实例的方法
        public LLWebSocket GetWebSocket(string key)
        {
            if (webSockets.ContainsKey(key))
            {
                return webSockets[key];
            }
            return null;
        }
    }
}