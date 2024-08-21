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
        ///��ʼ��
        ///</summary>
        public void Initialized()
        {
            // ��ȡ��˷��豸�б�
            string[] devices = Microphone.devices;
            if (devices.Length > 0)
            {
                microphoneDevice = null;
                Debug.Log("ʹ����˷��豸: " + microphoneDevice);
            }
            else
            {
                Debug.LogError("û���ҵ���˷��豸");
            }
            StartCoroutine(InitializedMicrophone());
        }

        ///<summary>
        ///��ʼ����˷�
        ///</summary>
        IEnumerator InitializedMicrophone()
        {
            yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
            if (Application.HasUserAuthorization(UserAuthorization.Microphone))
            {
                StartRecording();
                // ��ʼ¼��
                //microphoneClip = Microphone.Start(null, true, 1, 16000);

                //// �ȴ�¼�����
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
                Debug.Log("����Ȩ��˷�Ȩ�ޣ�");
            }
        }




        // ��ʼ¼��
        public void StartRecording()
        {
            if (!isRecording)
            {
                recordingClip = Microphone.Start(microphoneDevice, true, 1, AudioConfig.RATE);
                isRecording = true;
                Debug.Log("��ʼ¼��"+ isRecording);
                StartCoroutine(CheckAudioData());
            }
        }

        // ����¼��
        public void StopRecording()
        {
            if (isRecording)
            {
                Microphone.End(microphoneDevice);
                isRecording = false;
                Debug.Log("����¼��");
            }
        }

        /// <summary>
        /// ��һ�β���λ��
        /// </summary>
        int lastSampling;

        float[] f = new float[16000];
        IEnumerator CheckAudioData()
        {
            while (isRecording)
            {
                yield return new WaitForSeconds(0.005f);
                //if (!isRecording) continue;������Ŀ������ˣ���Ҫ��ʲôһЩû�У�������������û�а�������߲�ѯ�Ĳ��ܰ������ǵģ����ܸ�����������ʲô�ţ��ԶԶԣ���û�����
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
                    Debug.LogError("¼��δ��ʼ��¼���ѽ���");
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
        //            //Debug.Log($"¼�����ݳ��ȣ�{length}");
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
        //            Debug.LogError("¼��δ��ʼ��¼���ѽ���");
        //            break;
        //        }
        //    }
        //}

        void SendAudioData(byte[] data)
        {
            Debug.LogError($"�������ݣ�{data.Length}");
            LLWebSocket.Instance.Send(data);
        }

        public byte[] ConvertFloatToPCM(float[] floatSamples, out byte[] pcmData)
        {
            // PCM���ݵĲ����ʺ�ͨ����
            //const int sampleRate = AudioConfig.RATE;
            const int channels = 1; // ������

            // ����PCM���ݵĳ���
            int numSamples = floatSamples.Length * channels;
            pcmData = new byte[numSamples * 2]; // ÿ������16λ

            for (int i = 0; i < floatSamples.Length; i++)
            {
                short sample = (short)(floatSamples[i] * short.MaxValue);
                byte[] bytes = BitConverter.GetBytes(sample);

                // ���ϵͳ�ֽ�����Ŀ��PCM���ݵ��ֽ���ͬ������Ҫ�����ֽ�˳��
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(bytes);
                }

                pcmData[i * 2] = bytes[0];
                pcmData[i * 2 + 1] = bytes[1];
            }
            return pcmData;
        }

        // ת������
        public void ConvertFloatToPCM16(float[] floatArray, out byte[] pcmData)
        {
            int sampleCount = floatArray.Length;
            pcmData = new byte[sampleCount * 2]; // 16λPCM��ÿ������2�ֽ�

            for (int i = 0; i < sampleCount; i++)
            {
                // ��floatֵת��Ϊ16λPCMֵ
                // ע�⣺floatֵ��Χ��-1��1����16λPCMֵ��Χ��-32768��32767
                // ��ˣ�������Ҫ�������ź�ƫ��
                short pcmValue = (short)(floatArray[i] * short.MaxValue);

                // ��shortֵת��Ϊ�ֽ�����
                byte[] bytes = BitConverter.GetBytes(pcmValue);

                // ע�⣺BitConverter���ֽ�˳�����������ϵͳ�ܹ�����˻�С�ˣ�
                // �����Ҫȷ���ֽ�˳�����������紫�䣩���뿼��ʹ��Array.Reverse
                // ����������ǲ���Ҫ�ı��ֽ�˳��

                // ���ֽڸ��Ƶ����������
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
        /// �����������λΪ��
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