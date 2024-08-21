using LLStar.Net;
using LLVoice.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Net.WebSockets;
using UnityEngine;

namespace LLVoice.Voice
{
    public class LLVoiceManager : MonoSingleton<LLVoiceManager>
    {
        public AudioClip microphoneClip;
        /// <summary>
        /// 采样麦克风间隔时间
        /// </summary>
        public WaitForSeconds samplingInterval = new WaitForSeconds(1 / 5f);
        /// <summary>
        /// 是否正在录音
        /// </summary>
        public bool IsRecording = true;


        ///<summary>
        ///初始化
        ///</summary>
        public void Initialized()
        {
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
                //LLMicrophoneRecorder.Instance.StartRecording();
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

        /// <summary>
        /// 上一次采样位置
        /// </summary>
        int lastSampling;

        float[] f = new float[16000];
        IEnumerator MicrophoneSamplingRecognition()
        {
            Debug.Log($"开始录音: {IsRecording}");
            while (true)
            {
                yield return samplingInterval;
                if (!IsRecording)
                    continue;
                
                int currentPos = Microphone.GetPosition(null);
                bool isSucceed = microphoneClip.GetData(f, 0);

                if (isSucceed)
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
                            count = 16000 - lastSampling;
                            p = new float[count + currentPos];
                            Array.Copy(f, lastSampling, p, 0, count);

                            Array.Copy(f, 0, p, count, currentPos);

                            count += currentPos;
                        }

                        lastSampling = currentPos;
                        SendAudioToWebsocket(p);
                    }
            }
        }

        private void SendAudioToWebsocket(float[] p)
        {
            var buffer = FloatArrayToByteArray(p);
            LLWebSocket.Instance.Send(buffer);
        }

        byte[] FloatArrayToByteArray(in float[] floatArray)
        {
            int byteCount = floatArray.Length * sizeof(float);
            byte[] byteArray = new byte[byteCount];

            Buffer.BlockCopy(floatArray, 0, byteArray, 0, byteCount);

            return byteArray;
        }

        static byte[] Compress(in byte[] data)
        {
            using (MemoryStream compressedStream = new MemoryStream())
            {
                using (GZipStream gzipStream = new GZipStream(compressedStream, CompressionMode.Compress))
                {
                    gzipStream.Write(data, 0, data.Length);
                }
                return compressedStream.ToArray();
            }
        }
    }
}