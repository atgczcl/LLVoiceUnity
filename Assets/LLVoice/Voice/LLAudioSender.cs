using System;
using UnityEngine;
using System.Collections;
using LLVoice.Voice;
using LLStar.Net;

public class LLAudioSender : MonoBehaviour
{
    private float[] audioBuffer;
    private int sampleRate = 16000;
    private int frameSize = 25; // 每次发送25 ms的数据
    private AudioClip recordingClip;
    private int samplesPerFrame = 16000; // 每帧的样本数
    private int sampleCount = 0; // 当前缓冲区中的样本计数
    private string deviceName = Microphone.devices[0]; // 获取第一个可用的麦克风设备

    private void Start()
    {
        recordingClip = Microphone.Start(deviceName, true, 1, AudioConfig.RATE);
        samplesPerFrame = sampleRate * frameSize / 1000; // 每帧的样本数
        audioBuffer = new float[sampleRate * 2]; // 假设缓冲区足够大
        StartCoroutine(SendAudioFrames());
    }

    private IEnumerator SendAudioFrames()
    {
        while (true)
        {
            // 获取新的音频样本
            float[] newSamples = GetNewAudioSamples(samplesPerFrame);

            // 将新样本添加到缓冲区
            AddSamplesToBuffer(newSamples);

            // 检查是否有足够的样本发送
            if (sampleCount >= samplesPerFrame)
            {
                SendAudioFrame();
                sampleCount -= samplesPerFrame;
            }

            // 等待一段时间再继续处理
            yield return new WaitForSeconds(0.025f); // 每25 ms发送一次
        }
    }

    private float[] GetNewAudioSamples(int sampleCount)
    {
        // 获取新的音频样本
        int position = Microphone.GetPosition(deviceName) - sampleCount;
        float[] samples = new float[sampleCount];
        recordingClip.GetData(samples, 0);
        //Microphone.devices[0].get(deviceName, position, samples, sampleCount, sampleRate);
        return samples;
    }

    private void AddSamplesToBuffer(float[] newSamples)
    {
        // 将新样本添加到缓冲区
        for (int i = 0; i < newSamples.Length; i++)
        {
            audioBuffer[sampleCount] = newSamples[i];
            sampleCount++;
        }
    }

    private void SendAudioFrame()
    {
        // 发送当前帧的音频数据
        float[] frame = new float[samplesPerFrame];
        Array.Copy(audioBuffer, frame, samplesPerFrame);
        byte[] pcmBuffer = new byte[samplesPerFrame * 2];
        ConvertFloatToPCM(pcmBuffer, frame, samplesPerFrame);
        LLWebSocket.Instance.Send(pcmBuffer);
        // 发送到服务器或其他目的地
        Debug.Log("Sending frame with " + frame.Length + " samples.");
    }

    private void ConvertFloatToPCM(byte[] pcmBuffer, float[] floatSamples, int sampleCount)
    {
        for (int i = 0; i < sampleCount; i++)
        {
            short sample = (short)(floatSamples[i] * short.MaxValue);
            byte[] bytes = BitConverter.GetBytes(sample);

            // 如果系统字节序与目标PCM数据的字节序不同，则需要交换字节顺序
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            pcmBuffer[i * 2] = bytes[0];
            pcmBuffer[i * 2 + 1] = bytes[1];
        }
    }
}