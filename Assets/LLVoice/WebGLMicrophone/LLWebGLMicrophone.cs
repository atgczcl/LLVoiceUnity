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

        [DllImport("__Internal")]
        //JS_Microphone_IsCanSendData
        private static extern void JS_Microphone_IsCanSendData(bool isCanSend);
#else

        private static void JS_Microphone_Start() { }

        private static void JS_Microphone_Stop() { }

        private static void JS_Microphone_IsCanSendData(bool isCanSend) { }
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

        /// <summary>
        /// 设置是否可以发送数据
        /// </summary>
        public void JS_SetIsCanSendData(bool isCanSend)
        {
            JS_Microphone_IsCanSendData(isCanSend);
        }

        /// <summary>
        /// 暂时不用
        /// </summary>
        /// <param name="data"></param>
        public void Received_JS_Microphone_Data(string data)
        {
            // 从 IntPtr 获取数据
            //byte[] bytes = new byte[length];
            //Marshal.Copy(data, bytes, 0, length);
            byte[] bytes = System.Convert.FromBase64String(data);
            Debug.Log("Received bytes: " + data.Length);

            // 处理数据
            //LLWebSocket.Instance.Send(bytes);
        }
    }
}