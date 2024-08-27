using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using LLVoice.Tools;
using LLVoice.Net;
using Unity.VisualScripting;
using System.Threading;

namespace LLVoice.Voice
{
    /// <summary>
    /// 麦克风录音管理器
    /// webgl:https://github.com/bnco-dev/unity-webgl-microphone.git
    /// 
    /// </summary>
    public class LLMicrophoneRecorderMgr : MonoSingleton<LLMicrophoneRecorderMgr>
    {
        public AudioClip recordingClip;
        public string microphoneDevice;
        public bool isRecording = false;

        public override void Awake()
        {
            base.Awake();
            
        }

        ///<summary>
        ///初始化
        ///</summary>
        public void Initialized()
        {
            Debug.Log("初始化麦克风，开始！");
            //因为初始化是在websocket连接成功后进行的，在异步中无法调用协程，所以使用InvokeOnMainThread回到主线程执行
            InvokeOnMainThread(() => {
                StartCoroutine(InitializedMicrophone());
            });
        }

        ///<summary>
        ///初始化麦克风
        ///</summary>
        IEnumerator InitializedMicrophone()
        {
            yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
            if (Application.HasUserAuthorization(UserAuthorization.Microphone))
            {
                StartRecording();
            }
            else
            {
                Debug.Log("请授权麦克风权限！");
            }
            Debug.Log("初始化麦克风，完成！");
        }

        // 开始录音
        public void StartRecording()
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                LLWebGLMicrophone.Instance.JS_StartMicrophone();
            }
            else {
#if !UNITY_WEBGL
                // 获取麦克风设备列表
                string[] devices = Microphone.devices;
                Debug.Log("使用麦克风设备000: " + devices.Length);
                if (devices.Length > 0)
                {
                    microphoneDevice = null;
                    Debug.Log("使用麦克风设备: " + microphoneDevice);
                    if (!isRecording)
                    {
                        recordingClip = Microphone.Start(microphoneDevice, true, 1, AudioConfig.RATE);
                        isRecording = true;
                        Debug.Log("开始录音" + isRecording);
                        StartCoroutine(CheckAudioData());
                    }
                }
                else
                {
                    Debug.LogError("没有找到麦克风设备");
                }
                #endif
            }
        }

        // 结束录音
        public void StopRecording()
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                LLWebGLMicrophone.Instance.JS_StopMicrophone();
            }
            else { 
            #if !UNITY_WEBGL
                if (isRecording)
                {
                    Microphone.End(microphoneDevice);
                    isRecording = false;
                    Debug.Log("结束录音");
                }
            #endif
            }
        }
        #if !UNITY_WEBGL
        /// <summary>
        /// 上一次采样位置
        /// </summary>
        int lastSampling;

        float[] f = new float[16000];
        IEnumerator CheckAudioData()
        {
            while (isRecording)
            {
                yield return new WaitForSeconds(0.005f);
                //if (!isRecording) continue;部分项目需求成了，你要想什么一些没有，所以是晚安测试没有啊，你这边查询的不能啊，真是的，我能感受下来都是什么嗯，对对对，有没有你好
                if (recordingClip != null)
                {
                    int currentPos = Microphone.GetPosition(microphoneDevice);
                    bool isSucceed = recordingClip.GetData(f, 0);

                    if (isSucceed)
                    {
                        if (lastSampling != currentPos)
                        {
                            int count = 0;
                            float[] p = default;
                            if (currentPos > lastSampling)
                            {
                                count = currentPos - lastSampling;
                                p = new float[count];
                                Array.Copy(f, lastSampling, p, 0, count);
                            }
                            else
                            {
                                count = AudioConfig.RATE - lastSampling;
                                p = new float[count + currentPos];
                                Array.Copy(f, lastSampling, p, 0, count);
                                Array.Copy(f, 0, p, count, currentPos);
                                count += currentPos;
                            }

                            lastSampling = currentPos;
                            ConvertFloatToPCM16(p, out byte[] pcmData);
                            SendAudioData(pcmData);
                        }
                    }
                }
                else
                {
                    Debug.LogError("录音未开始或录音已结束");
                    break;
                }
            }
        }
#endif
        /// <summary>
        /// 开始录音
        /// </summary>
        /// <param name="data"></param>
        void SendAudioData(byte[] data)
        {
            //Debug.LogError($"发送数据：{data.Length}");
            LLWebSocket.Instance.Send(data);
        }

        /// <summary>
        /// 转换函数,将float数组转换为PCM16数据
        /// </summary>
        /// <param name="floatArray"></param>
        /// <param name="pcmData"></param>
        public void ConvertFloatToPCM16(float[] floatArray, out byte[] pcmData)
        {
            int sampleCount = floatArray.Length;
            pcmData = new byte[sampleCount * 2]; // 16位PCM，每个样本2字节

            for (int i = 0; i < sampleCount; i++)
            {
                // 将float值转换为16位PCM值
                // 注意：float值范围从-1到1，而16位PCM值范围从-32768到32767
                // 因此，我们需要进行缩放和偏移
                short pcmValue = (short)(floatArray[i] * short.MaxValue);

                // 将short值转换为字节数组
                byte[] bytes = BitConverter.GetBytes(pcmValue);

                // 注意：BitConverter的字节顺序可能依赖于系统架构（大端或小端）
                // 如果需要确保字节顺序（如用于网络传输），请考虑使用Array.Reverse
                // 这里假设我们不需要改变字节顺序

                // 将字节复制到结果数组中
                pcmData[i * 2] = bytes[0];
                pcmData[i * 2 + 1] = bytes[1];
            }
        }

        

        private void OnDestroy()
        {
            StopRecording();
        }
    }


    public class AudioConfig
    {
        public const short FORMAT = (short)1; // paInt16 in Python is represented as 1 in C#
        public const int CHANNELS = 1;
        public const int RATE = 16000;

        public static int[] ChunkSize = new int[] { 5, 10, 5 };
        /// <summary>
        /// 采样间隔，单位为秒
        /// </summary>
        public static int ChunkInterval = 10;

        public AudioConfig(int[] chunkSize, int chunkInterval)
        {
            ChunkSize = chunkSize;
            ChunkInterval = chunkInterval;
        }

        public static int CalculateChunkSize()
        {
            double chunkSize = 60 * ChunkSize[1] / ChunkInterval;
            int chunk = (int)(RATE / 1000 * chunkSize);
            return chunk;
        }
    }
}