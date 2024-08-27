using LLVoice.Net;
using LLVoice.Tools;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace LLVoice.Voice
{
    public class LLWebGLMicrophone : MonoSingleton<LLWebGLMicrophone>
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void JS_Microphone_Start();

        [DllImport("__Internal")]
        private static extern void JS_Microphone_Stop();
#else

        private static void JS_Microphone_Start() { }

        private static void JS_Microphone_Stop() { }
#endif

        /// <summary>
        /// 开始录音
        /// </summary>
        public void JS_StartMicrophone()
        {
            JS_Microphone_Start();
        }

        /// <summary>
        /// 停止录音
        /// </summary>
        public void JS_StopMicrophone()
        {
            JS_Microphone_Stop();
        }

        public void Received_JS_Microphone_Data(string data)
        {
            // 从 IntPtr 获取数据
            //byte[] bytes = new byte[length];
            //Marshal.Copy(data, bytes, 0, length);
            byte[] bytes = System.Convert.FromBase64String(data);
            Debug.Log("Received bytes: " + data.Length);

            // 处理数据
            LLWebSocket.Instance.Send(bytes);
        }
    }
}