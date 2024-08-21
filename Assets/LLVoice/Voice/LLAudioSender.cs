using System;
using UnityEngine;
using System.Collections;
using LLVoice.Voice;
using LLStar.Net;

public class LLAudioSender : MonoBehaviour
{
    private float[] audioBuffer;
    private int sampleRate = 16000;
    private int frameSize = 25; // ÿ�η���25 ms������
    private AudioClip recordingClip;
    private int samplesPerFrame = 16000; // ÿ֡��������
    private int sampleCount = 0; // ��ǰ�������е���������
    private string deviceName = Microphone.devices[0]; // ��ȡ��һ�����õ���˷��豸

    private void Start()
    {
        recordingClip = Microphone.Start(deviceName, true, 1, AudioConfig.RATE);
        samplesPerFrame = sampleRate * frameSize / 1000; // ÿ֡��������
        audioBuffer = new float[sampleRate * 2]; // ���軺�����㹻��
        StartCoroutine(SendAudioFrames());
    }

    private IEnumerator SendAudioFrames()
    {
        while (true)
        {
            // ��ȡ�µ���Ƶ����
            float[] newSamples = GetNewAudioSamples(samplesPerFrame);

            // ����������ӵ�������
            AddSamplesToBuffer(newSamples);

            // ����Ƿ����㹻����������
            if (sampleCount >= samplesPerFrame)
            {
                SendAudioFrame();
                sampleCount -= samplesPerFrame;
            }

            // �ȴ�һ��ʱ���ټ�������
            yield return new WaitForSeconds(0.025f); // ÿ25 ms����һ��
        }
    }

    private float[] GetNewAudioSamples(int sampleCount)
    {
        // ��ȡ�µ���Ƶ����
        int position = Microphone.GetPosition(deviceName) - sampleCount;
        float[] samples = new float[sampleCount];
        recordingClip.GetData(samples, 0);
        //Microphone.devices[0].get(deviceName, position, samples, sampleCount, sampleRate);
        return samples;
    }

    private void AddSamplesToBuffer(float[] newSamples)
    {
        // ����������ӵ�������
        for (int i = 0; i < newSamples.Length; i++)
        {
            audioBuffer[sampleCount] = newSamples[i];
            sampleCount++;
        }
    }

    private void SendAudioFrame()
    {
        // ���͵�ǰ֡����Ƶ����
        float[] frame = new float[samplesPerFrame];
        Array.Copy(audioBuffer, frame, samplesPerFrame);
        byte[] pcmBuffer = new byte[samplesPerFrame * 2];
        ConvertFloatToPCM(pcmBuffer, frame, samplesPerFrame);
        LLWebSocket.Instance.Send(pcmBuffer);
        // ���͵�������������Ŀ�ĵ�
        Debug.Log("Sending frame with " + frame.Length + " samples.");
    }

    private void ConvertFloatToPCM(byte[] pcmBuffer, float[] floatSamples, int sampleCount)
    {
        for (int i = 0; i < sampleCount; i++)
        {
            short sample = (short)(floatSamples[i] * short.MaxValue);
            byte[] bytes = BitConverter.GetBytes(sample);

            // ���ϵͳ�ֽ�����Ŀ��PCM���ݵ��ֽ���ͬ������Ҫ�����ֽ�˳��
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            pcmBuffer[i * 2] = bytes[0];
            pcmBuffer[i * 2 + 1] = bytes[1];
        }
    }
}