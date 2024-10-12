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
    /// <param name="audioClip">要转换的AudioClip对象</param>
    /// <param name="outputPath">输出WAV文件的路径</param>
    public static void ConvertAudioClipToWAV(AudioClip audioClip, string outputPath)
    {
        // 获取AudioClip的基本信息
        int sampleRate = audioClip.frequency; // 采样率
        int numSamples = audioClip.samples; // 样本数量
        int numChannels = audioClip.channels; // 通道数
        int bitsPerSample = 16; // Unity默认使用16位

        // 计算PCM数据相关参数
        int samples = audioClip.samples;
        int bytesPerSample = bitsPerSample / 8; // 每样本字节数
        int bytesPerSecond = sampleRate * numChannels * bytesPerSample; // 每秒字节数
        int blockAlign = numChannels * bytesPerSample; // 块对齐
        int blockSize = samples * numChannels * bytesPerSample; // 数据块大小

        using (var stream = new FileStream(outputPath, FileMode.Create))
        {
            using (var writer = new BinaryWriter(stream))
            {
                // 写入WAV文件头
                writer.Write(Encoding.UTF8.GetBytes("RIFF")); // RIFF Chunk ID
                writer.Write(36 + samples * numChannels * bytesPerSample); // RIFF Chunk Size 
                writer.Write(Encoding.UTF8.GetBytes("WAVE")); // WAVE 格式
                writer.Write(Encoding.UTF8.GetBytes("fmt ")); // 子块1 ID
                writer.Write(16); // 子块1 大小
                writer.Write((ushort)1); // 音频格式 (1 = PCM)
                writer.Write((ushort)numChannels); // 通道数
                writer.Write(sampleRate); // 采样率
                writer.Write(bytesPerSecond); // 字节率
                writer.Write((ushort)blockAlign); // 块对齐
                writer.Write((ushort)bitsPerSample); // 每样本位数
                writer.Write(Encoding.UTF8.GetBytes("data")); // 子块2 ID
                writer.Write(blockSize); // 数据块大小

                // 写入PCM数据
                float[] audioData = new float[samples * numChannels]; // 创建PCM数据数组
                audioClip.GetData(audioData, 0); // 获取AudioClip中的PCM数据
                for (int i = 0; i < samples * numChannels; i++)
                {
                    writer.Write((short)(audioData[i] * short.MaxValue)); // 写入每个样本数据
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
        //ToWAV(ConvertPCM16ToAudioClip(pcmData, 22050), filePath);
        PcmToWav(pcmData, 22050, 16, 1, filePath);
    }

    /// <summary>
    /// 将PCM16数据转换为Wav数据
    /// </summary>
    /// <param name="pcmData">字节流</param>
    /// <param name="sampleRate">22050</param>
    /// <param name="bitsPerSample">16</param>
    /// <param name="numChannels">1</param>
    /// <returns></returns>
    public static void PcmToWav(byte[] pcmData, int sampleRate, int bitsPerSample, int numChannels, string outputPath)
    {
        // 计算PCM数据相关参数
        int samples = pcmData.Length / (numChannels * bitsPerSample / 8); // 样本数
        int bytesPerSample = bitsPerSample / 8; // 每样本字节数
        int bytesPerSecond = sampleRate * numChannels * bytesPerSample; // 每秒字节数
        int blockAlign = numChannels * bytesPerSample; // 块对齐
        int blockSize = samples * numChannels * bytesPerSample; // 数据块大小

        using (var stream = new FileStream(outputPath, FileMode.Create))
        {
            using (var writer = new BinaryWriter(stream))
            {
                // 写入WAV文件头
                writer.Write(Encoding.UTF8.GetBytes("RIFF")); // RIFF Chunk ID
                writer.Write(36 + samples * numChannels * bytesPerSample); // RIFF Chunk Size 
                writer.Write(Encoding.UTF8.GetBytes("WAVE")); // WAVE 格式
                writer.Write(Encoding.UTF8.GetBytes("fmt ")); // 子块1 ID
                writer.Write(16); // 子块1 大小
                writer.Write((ushort)1); // 音频格式 (1 = PCM)
                writer.Write((ushort)numChannels); // 通道数
                writer.Write(sampleRate); // 采样率
                writer.Write(bytesPerSecond); // 字节率
                writer.Write((ushort)blockAlign); // 块对齐
                writer.Write((ushort)bitsPerSample); // 每样本位数
                writer.Write(Encoding.UTF8.GetBytes("data")); // 子块2 ID
                writer.Write(blockSize); // 数据块大小

                // 写入PCM数据
                float[] audioData = ConvertPCM16ToFloatArray(pcmData).ToArray(); // 获取AudioClip中的PCM数据
                for (int i = 0; i < audioData.Length; i++)
                {
                    writer.Write((short)(audioData[i] * short.MaxValue)); // 写入每个样本数据
                }
            }
        }
    }

    /// <summary>
    /// 将PCM16数据转换为AudioClip
    /// </summary>
    /// <param name="pcmData"></param>
    /// <param name="sampleRate"></param>
    /// <returns></returns>
    public static AudioClip ConvertPCM16ToAudioClip(byte[] pcmData, int sampleRate)
    {
        //int sampleCount = pcmData.Length / 2; // 每个样本占用2字节
        //float[] floatArray = new float[sampleCount];
        List<float> floats = ConvertPCM16ToFloatArray(pcmData);

        // 创建AudioClip
        AudioClip audioClip = AudioClip.Create("PCMClip", floats.Count, 1, sampleRate, false);
        audioClip.SetData(floats.ToArray(), 0);

        return audioClip;
    }

    /// <summary>
    /// 将PCM数据转换为float数组
    /// </summary>
    /// <param name="pcmData"></param>
    /// <param name="sampleRate"></param>
    /// <returns></returns>
    public static List<float> ConvertPCM16ToFloatArray(byte[] pcmData)
    {
        int floatSize = sizeof(float);
        List<float> floats = new();

        for (int i = 0; i < pcmData.Length; i += floatSize)
        {
            // 检查是否有足够的字节来形成一个浮点数
            if (i + floatSize > pcmData.Length) break;

            // 解析字节到浮点数
            float value = BitConverter.ToSingle(pcmData, i);
            floats.Add(value);
        }
        return floats;
    }


    /// <summary>
    /// 将Unity的AudioClip转换为WAV文件
    /// </summary>
    /// <param name="audio"></param>
    /// <param name="path"></param>
    /// <returns></returns>
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