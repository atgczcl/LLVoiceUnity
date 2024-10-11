using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class LLAudioConverter
{
    /// <summary>
    /// ��AudioClipת��ΪWAV�ļ�
    /// </summary>
    /// <param name="audioClip">AudioClip����</param>
    /// <param name="outputPath">����ļ�·��</param>
    public static void ConvertAudioClipToWAV(AudioClip audioClip, string outputPath)
    {
        // ��ȡAudioClip�Ļ�����Ϣ
        int sampleRate = audioClip.frequency;
        int numSamples = audioClip.samples;
        int numChannels = audioClip.channels;
        int bitsPerSample = 16; // UnityĬ��ʹ��16λ

        // ��ȡPCM����
        float[] samples = new float[numSamples * numChannels];
        audioClip.GetData(samples, 0);

        // ����������ת��Ϊ16λ��������
        short[] pcmData = new short[samples.Length];
        for (int i = 0; i < samples.Length; i++)
        {
            pcmData[i] = (short)(samples[i] * short.MaxValue);
        }

        // ����WAV�ļ�
        WriteWavFile(pcmData, sampleRate, numChannels, bitsPerSample, outputPath);
    }

    /// <summary>
    /// д��WAV�ļ�
    /// </summary>
    /// <param name="pcmData">PCM����</param>
    /// <param name="sampleRate">������</param>
    /// <param name="numChannels">ͨ����</param>
    /// <param name="bitsPerSample">ÿ����λ��</param>
    /// <param name="outputPath">����ļ�·��</param>
    private static void WriteWavFile(short[] pcmData, int sampleRate, int numChannels, int bitsPerSample, string outputPath)
    {
        int bytesPerSample = bitsPerSample / 8;
        int bytesPerSecond = sampleRate * numChannels * bytesPerSample;
        int byteRate = bytesPerSecond * (bitsPerSample / 8);
        int blockAlign = numChannels * bytesPerSample;

        using (var stream = new FileStream(outputPath, FileMode.Create))
        {
            using (var writer = new BinaryWriter(stream))
            {
                // д��WAV�ļ�ͷ
                writer.Write(Encoding.UTF8.GetBytes("RIFF")); // RIFF Chunk ID
                writer.Write(36 + pcmData.Length * bytesPerSample); // RIFF Chunk Size
                writer.Write(Encoding.UTF8.GetBytes("WAVE")); // WAVE Format
                writer.Write(Encoding.UTF8.GetBytes("fmt ")); // Subchunk 1 ID
                writer.Write(16); // Subchunk 1 Size
                writer.Write((short)1); // Audio Format (1 = PCM)
                writer.Write((short)numChannels); // Number of Channels
                writer.Write(sampleRate); // Sample Rate
                writer.Write(bytesPerSecond); // Byte Rate
                writer.Write(blockAlign); // Block Align
                writer.Write((short)bitsPerSample); // Bits per Sample
                writer.Write(Encoding.UTF8.GetBytes("data")); // Subchunk 2 ID
                writer.Write(pcmData.Length * bytesPerSample); // Subchunk 2 Size

                // д��PCM����
                foreach (short sample in pcmData)
                {
                    writer.Write(sample);
                }
            }
        }
    }

    /// <summary>
    /// ��wav���ݱ��浽�ļ�
    /// </summary>
    /// <param name="wavData"></param>
    /// <param name="filePath"></param>
    public static void SaveWavToFile(byte[] wavData, string filePath)
    {
        File.WriteAllBytes(filePath, wavData);
    }

    /// <summary>
    /// ��pcm���ݱ��浽�ļ�
    /// </summary>
    /// <param name="pcmData"></param>
    /// <param name="filePath"></param>
    public static void SavePcmToFile(byte[] pcmData, string filePath)
    {
        File.WriteAllBytes(filePath, pcmData);
    }

    /// <summary>
    /// ��pcm���ݱ��浽Wav�ļ�
    /// </summary>
    /// <param name="pcmData"></param>
    /// <param name="filePath"></param>
    public static void SavePcmToWavFile(byte[] pcmData, string filePath)
    {
        //File.WriteAllBytes(filePath, PcmToWav(pcmData, 22050, 16, 1));
        //AudioConverter.ConvertPcmToWavN((pcmData), 22050, 1, 32, filePath);
        ConvertPcmToWav(pcmData, filePath);
    }

    public static void ConvertPcmToWav(byte[] pcmData, string filePath)
    {
        ConvertAudioClipToWAV(ConvertPCM16ToAudioClip(pcmData, 22050), filePath);
    }

    public static AudioClip ConvertPCM16ToAudioClip(byte[] pcmData, int sampleRate)
    {
        int sampleCount = pcmData.Length / 2; // ÿ������ռ��2�ֽ�
        float[] floatArray = new float[sampleCount];

        int floatSize = sizeof(float);
        List<float> floats = new List<float>();

        for (int i = 0; i < pcmData.Length; i += floatSize)
        {
            // ����Ƿ����㹻���ֽ����γ�һ��������
            if (i + floatSize > pcmData.Length) break;

            // �����ֽڵ�������
            float value = BitConverter.ToSingle(pcmData, i);
            floats.Add(value);
        }

        // ����AudioClip
        AudioClip audioClip = AudioClip.Create("PCMClip", floats.Count, 1, sampleRate, false);
        audioClip.SetData(floats.ToArray(), 0);

        return audioClip;
    }


    public static bool ToWAV(AudioClip audio, string path)
    {
        try
        {
            int samples = audio.samples;
            int channels = audio.channels;
            int sampleRate = audio.frequency;

            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    // д��WAV�ļ�ͷ
                    writer.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"));
                    writer.Write(36 + samples * channels * 2); // �ܴ�С��16λPCM��Ҫ����2
                    writer.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"));

                    // д���ʽ��
                    writer.Write(System.Text.Encoding.UTF8.GetBytes("fmt "));
                    writer.Write(16); // �ӿ��С
                    writer.Write((ushort)1); // PCM��ʽ
                    writer.Write((ushort)channels); // ͨ����
                    writer.Write(sampleRate); // ������
                    writer.Write(sampleRate * channels * 2); // ÿ���ֽ���
                    writer.Write((ushort)(channels * 2)); // ���ݿ����
                    writer.Write((ushort)16); // ÿ��������λ��

                    // д�����ݿ�
                    writer.Write(System.Text.Encoding.UTF8.GetBytes("data"));
                    writer.Write(samples * channels * 2); // ���ݿ��С

                    // д����Ƶ����
                    float[] audioData = new float[samples * channels];
                    audio.GetData(audioData, 0);
                    for (int i = 0; i < audioData.Length; i++)
                    {
                        short sample = (short)(audioData[i] * short.MaxValue); // ��������ת��Ϊ16λ����
                        writer.Write(sample);
                    }
                }
            }
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"��Ƶ����ʧ��: {e.Message}");
            return false;
        }
    }
}