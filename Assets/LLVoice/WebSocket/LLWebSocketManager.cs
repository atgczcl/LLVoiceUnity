using LLVoice.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LLVoice.Net
{
    /// <summary>
    /// WebSocket������
    /// </summary>
    public class LLWebSocketManager : MonoSingleton<LLWebSocketManager>
    {
        public readonly Dictionary<string, LLWebSocket> webSockets = new();

        /// <summary>
        /// ���WebSocket
        /// </summary>
        /// <param name="key">websocket ID key</param>
        /// <param name="url"> websocket url:ws://192.168.1.1:8080, wss://192.168.1.1:8080</param>
        /// <param name="onConnect">���ӳɹ��ص���ע��Ĭ���Ѿ��������л����̵߳��ã����Ե���unity���̷߳���</param>
        /// <param name="onStrMsg">�յ��ַ�����Ϣ�ص�, ͨ��Update���͹����������߳�</param>
        /// <param name="onByteMsg">�յ��ֽ���Ϣ�ص�, ͨ��Update���͹����������߳�</param>
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
        /// ������Ϣstring
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
        /// ������Ϣbyte[]
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

        // ������ȡ WebSocket ʵ���ķ���
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