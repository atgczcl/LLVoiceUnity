using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class LLAudioConverter
{
    /// <summary>
    /// 将AudioClip转换为WAV文件
    /// </summary>
    /// <param name="audioClip">AudioClip对象</param>
    /// <param name="outputPath">输出文件路径</param>
    public static void ConvertAudioClipToWAV(AudioClip audioClip, string outputPath)
    {
        // 获取AudioClip的基本信息
        int sampleRate = audioClip.frequency;
        int numSamples = audioClip.samples;
        int numChannels = audioClip.channels;
        int bitsPerSample = 16; // Unity默认使用16位

        // 获取PCM数据
        float[] samples = new float[numSamples * numChannels];
        audioClip.GetData(samples, 0);

        // 将浮点数据转换为16位整数数据
        short[] pcmData = new short[samples.Length];
        for (int i = 0; i < samples.Length; i++)
        {
            pcmData[i] = (short)(samples[i] * short.MaxValue);
        }

        // 创建WAV文件
        WriteWavFile(pcmData, sampleRate, numChannels, bitsPerSample, outputPath);
    }

    /// <summary>
    /// 写入WAV文件
    /// </summary>
    /// <param name="pcmData">PCM数据</param>
    /// <param name="sampleRate">采样率</param>
    /// <param name="numChannels">通道数</param>
    /// <param name="bitsPerSample">每样本位数</param>
    /// <param name="outputPath">输出文件路径</param>
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
                // 写入WAV文件头
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

                // 写入PCM数据
                foreach (short sample in pcmData)
                {
                    writer.Write(sample);
                }
            }
        }
    }

    /// <summary>
    /// 将wav数据保存到文件
    /// </summary>
    /// <param name="wavData"></param>
    /// <param name="filePath"></param>
    public static void SaveWavToFile(byte[] wavData, string filePath)
    {
        File.WriteAllBytes(filePath, wavData);
    }

    /// <summary>
    /// 将pcm数据保存到文件
    /// </summary>
    /// <param name="pcmData"></param>
    /// <param name="filePath"></param>
    public static void SavePcmToFile(byte[] pcmData, string filePath)
    {
        File.WriteAllBytes(filePath, pcmData);
    }

    /// <summary>
    /// 将pcm数据保存到Wav文件
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
        int sampleCount = pcmData.Length / 2; // 每个样本占用2字节
        float[] floatArray = new float[sampleCount];

        int floatSize = sizeof(float);
        List<float> floats = new List<float>();

        for (int i = 0; i < pcmData.Length; i += floatSize)
        {
            // 检查是否有足够的字节来形成一个浮点数
            if (i + floatSize > pcmData.Length) break;

            // 解析字节到浮点数
            float value = BitConverter.ToSingle(pcmData, i);
            floats.Add(value);
        }

        // 创建AudioClip
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
                    // 写入WAV文件头
                    writer.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"));
                    writer.Write(36 + samples * channels * 2); // 总大小，16位PCM需要乘以2
                    writer.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"));

                    // 写入格式块
                    writer.Write(System.Text.Encoding.UTF8.GetBytes("fmt "));
                    writer.Write(16); // 子块大小
                    writer.Write((ushort)1); // PCM格式
                    writer.Write((ushort)channels); // 通道数
                    writer.Write(sampleRate); // 采样率
                    writer.Write(sampleRate * channels * 2); // 每秒字节数
                    writer.Write((ushort)(channels * 2)); // 数据块对齐
                    writer.Write((ushort)16); // 每个样本的位数

                    // 写入数据块
                    writer.Write(System.Text.Encoding.UTF8.GetBytes("data"));
                    writer.Write(samples * channels * 2); // 数据块大小

                    // 写入音频数据
                    float[] audioData = new float[samples * channels];
                    audio.GetData(audioData, 0);
                    for (int i = 0; i < audioData.Length; i++)
                    {
                        short sample = (short)(audioData[i] * short.MaxValue); // 将浮点数转换为16位整数
                        writer.Write(sample);
                    }
                }
            }
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"音频保存失败: {e.Message}");
            return false;
        }
    }
}