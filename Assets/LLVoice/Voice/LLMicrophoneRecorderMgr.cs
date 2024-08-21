using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Threading;
using LLStar.Net;
using LLVoice.Tools;
using UnityEngine.UI;

namespace LLVoice.Voice
{
    public class LLMicrophoneRecorderMgr : MonoSingleton<LLMicrophoneRecorderMgr>
    {
        public AudioClip recordingClip;
        public string microphoneDevice;
        public bool isRecording = false;

        ///<summary>
        ///初始化
        ///</summary>
        public void Initialized()
        {
            // 获取麦克风设备列表
            string[] devices = Microphone.devices;
            if (devices.Length > 0)
            {
                microphoneDevice = null;
                Debug.Log("使用麦克风设备: " + microphoneDevice);
            }
            else
            {
                Debug.LogError("没有找到麦克风设备");
            }
            StartCoroutine(InitializedMicrophone());
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
                // 开始录音
                //microphoneClip = Microphone.Start(null, true, 1, 16000);

                //// 等待录音完成
                //while (!(Microphone.GetPosition(null) >= microphoneClip.length))
                //{
                //    yield return null;
                //}

                //do
                //{
                //    microphoneClip = Microphone.Start(null, true, 1, 16000);
                //    yield return null;
                //} while (!Microphone.IsRecording(null));

                //StartCoroutine(MicrophoneSamplingRecognition());
            }
            else
            {
                Debug.Log("请授权麦克风权限！");
            }
        }




        // 开始录音
        public void StartRecording()
        {
            if (!isRecording)
            {
                recordingClip = Microphone.Start(microphoneDevice, true, 1, AudioConfig.RATE);
                isRecording = true;
                Debug.Log("开始录音"+ isRecording);
                StartCoroutine(CheckAudioData());
            }
        }

        // 结束录音
        public void StopRecording()
        {
            if (isRecording)
            {
                Microphone.End(microphoneDevice);
                isRecording = false;
                Debug.Log("结束录音");
            }
        }

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
                    int currentPos = Microphone.GetPosition(null);
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

        byte[] FloatArrayToByteArray(in float[] floatArray)
        {
            int byteCount = floatArray.Length * sizeof(float);
            byte[] byteArray = new byte[byteCount];

            Buffer.BlockCopy(floatArray, 0, byteArray, 0, byteCount);

            return byteArray;
        }

        //IEnumerator CheckAudioData()
        //{
        //    while (isRecording)
        //    {
        //        yield return new WaitForSeconds(0.01f);
        //        if (isRecording && recordingClip != null)
        //        {
        //            int length = recordingClip.samples * recordingClip.channels;
        //            //int length = AudioConfig.CalculateChunkSize();
        //            //Debug.Log($"录音数据长度：{length}");
        //            float[] floatArray = new float[length];
        //            recordingClip.GetData(floatArray, 0);
        //            Debug.LogError($"recordingClip.samples:{recordingClip.samples}");
        //            //byte[] byteArray = ConvertFloatToPCM(floatArray);
        //            ConvertFloatToPCM16(floatArray, out byte[] byteArray);

        //            ///////////////////////////////pcm////////////////////////////////////////
        //            //byte[] bytes = new byte[floatArray.Length * 2];
        //            //int byteCount = floatArray.Length * 2;
        //            //byte[] byteArray = new byte[byteCount];

        //            //int offset = 0;
        //            //foreach (float sample in floatArray)
        //            //{
        //            //    short convertedSample = (short)(sample * short.MaxValue);
        //            //    byteArray[offset++] = (byte)(convertedSample & 0xff);
        //            //    byteArray[offset++] = (byte)((convertedSample >> 8) & 0xff);
        //            //}
        //            //////////////////////////////////////////////////////////////////////////////////

        //            //frameCount++;

        //            //if (frameCount >= SEND_INTERVAL)
        //            //{
        //            //    frameCount = 0;
        //            //    SendAudioData(bytes);
        //            //}

        //            SendAudioData(byteArray);
        //        }
        //        else
        //        {
        //            Debug.LogError("录音未开始或录音已结束");
        //            break;
        //        }
        //    }
        //}

        void SendAudioData(byte[] data)
        {
            Debug.LogError($"发送数据：{data.Length}");
            LLWebSocket.Instance.Send(data);
        }

        public byte[] ConvertFloatToPCM(float[] floatSamples, out byte[] pcmData)
        {
            // PCM数据的采样率和通道数
            //const int sampleRate = AudioConfig.RATE;
            const int channels = 1; // 单声道

            // 计算PCM数据的长度
            int numSamples = floatSamples.Length * channels;
            pcmData = new byte[numSamples * 2]; // 每个样本16位

            for (int i = 0; i < floatSamples.Length; i++)
            {
                short sample = (short)(floatSamples[i] * short.MaxValue);
                byte[] bytes = BitConverter.GetBytes(sample);

                // 如果系统字节序与目标PCM数据的字节序不同，则需要交换字节顺序
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(bytes);
                }

                pcmData[i * 2] = bytes[0];
                pcmData[i * 2 + 1] = bytes[1];
            }
            return pcmData;
        }

        // 转换函数
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
            if (isRecording)
            {
                StopRecording();
            }
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