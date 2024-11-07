using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Collections;
using LLVoice.Tools;
using System;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using Unity.VisualScripting;
using System.Threading.Tasks;
using System.Linq;

public class LLTTSManager : MonoSingleton<LLTTSManager>
{
    public AudioSource audioSource;
    public static bool IsDestroy = false;
    //RequestIdList
    public LLTTSRequestStopGenerationBlock RequestIdList = new ();
    public string TTSUrl = "http://127.0.0.1:8080/";

    public override void Awake()
    {
        base.Awake();
        IsDestroy = false;
        if (gameObject.TryGetComponent<AudioSource>(out audioSource) == false)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnApplicationQuit()
    {
        //StopTTSGeneration();
    }

    private void OnDestroy()
    {
        IsDestroy = true;
    }

    /// <summary>
    /// 合成语音
    /// </summary>
    /// <param name="url">http://127.0.0.1:8080/v1/tts</param>
    /// <param name="text"></param>
    /// <param name="speaker">中文女,中文男,英文女,英文男,日语男,粤语女,韩语女</param>
    /// <param name="complete"></param>
    /// <returns></returns>
    public Coroutine SendTTSRequest(string url, string text, string speaker, Action<AudioClip> complete)
    {
        return StartCoroutine(OnSendTTSRequest(url, text, speaker, complete));
    }

    public IEnumerator OnSendTTSRequest(string url, string text, string speaker, Action<AudioClip> complete)
    {
        var wwwForm = new WWWForm();
        wwwForm.AddField("text", text);
        wwwForm.AddField("spk", speaker);

        using (var request = UnityWebRequestMultimedia.GetAudioClip($"{TTSUrl}tts?text={text}&spk={speaker}", AudioType.WAV))
        {
            yield return request.SendWebRequest();
            if (request.isDone)
            {
                AudioClip _audioClip = DownloadHandlerAudioClip.GetContent(request);
                complete?.Invoke(_audioClip);
            }
            else
            {
                Debug.LogError($"ReadStreamFile Error:" + request.error);
            }
        }
    }

    /// <summary>
    /// 合成语音
    /// </summary>
    /// <param name="content"></param>
    /// <param name="speeker">['中文女', '中文男', '日语男', '粤语女', '英文女', '英文男', '韩语女']</param>
    /// <param name="onResult"></param>
    public async void GetTTSStream_SFT(string content, Action<AudioClip> onResult, string speeker = null)
    {
        Dictionary<string, string> formData = new();
        //  {
        //      query: data,
        //  prompt_text: '确保已部署CosyVoice项目，已将 CosyVoice-api中的api.py放入，并成功启动了 api.py。',
        //  prompt_speech: "yy.wav"
        //}
        //            //string cc = @"中新网北京9月29日电 (记者 高凯)据灯塔专业版数据，截至9月29日14时38分，2024年国庆档(10月1日—10月7日)档期内预售总票房突破1亿元，《749局》《浴火之路》《志愿军：存亡之战》暂列档期预售票房榜前三位。
        //根据灯塔专业版上映日历显示，今年国庆档将有10部新片上映，涵盖剧情、战争、犯罪、动作、灾难、家庭、科幻、冒险、青春、历史、歌舞、动画、喜剧、奇幻等多种类型，其中真人影片8部，不仅在数量上比去年国庆档多2部，并且都是中上制作规模的影片，也是近几年头部及中腰部影片数量最多的一次国庆档。";
        string cc = @"中新网北京9月29日电 (记者 高凯)据灯塔专业版数据，截至9月29日14时38分，2024年国庆档(10月1日—10月7日)档期内预售总票房突破1亿元，《749局》《浴火之路》《志愿军：存亡之战》暂列档期预售票房榜前三位。";
        formData?.TryAdd("query", content);
        formData?.TryAdd("speaker", speeker == null ? "中文女" : speeker);
        //speed = question_data.get('speed', 1.0)
        //isStream = question_data.get('isStream', False)
        formData?.TryAdd("speed", 0.8f.ToString());
        formData?.TryAdd("isStream", false.ToString());

        //formData?.TryAdd("prompt_speech", "yy.wav");
        string jsonData = JsonConvert.SerializeObject(formData);
        string url = $"{TTSUrl}inference/stream_sft";
        await PostTTSStream(url, jsonData, pcmData => {
            AudioClip audioClip = ConvertPCM16ToAudioClip(pcmData, 22050);
            onResult?.Invoke(audioClip);
        });
    }

    /// <summary>
    /// 服务器没有完善，需要修改代码
    /// </summary>
    /// <param name="content"></param>
    /// <param name="onResult"></param>
    public async void GetTTSStream_Clone(string content, Action<AudioClip> onResult)
    {
        Dictionary<string, string> formData = new();
        //  {
        //      query: data,
        //  prompt_text: '确保已部署CosyVoice项目，已将 CosyVoice-api中的api.py放入，并成功启动了 api.py。',
        //  prompt_speech: "yy.wav"
        //}
        //            //string cc = @"中新网北京9月29日电 (记者 高凯)据灯塔专业版数据，截至9月29日14时38分，2024年国庆档(10月1日—10月7日)档期内预售总票房突破1亿元，《749局》《浴火之路》《志愿军：存亡之战》暂列档期预售票房榜前三位。
        //根据灯塔专业版上映日历显示，今年国庆档将有10部新片上映，涵盖剧情、战争、犯罪、动作、灾难、家庭、科幻、冒险、青春、历史、歌舞、动画、喜剧、奇幻等多种类型，其中真人影片8部，不仅在数量上比去年国庆档多2部，并且都是中上制作规模的影片，也是近几年头部及中腰部影片数量最多的一次国庆档。";
        string cc = @"中新网北京9月29日电 (记者 高凯)据灯塔专业版数据，截至9月29日14时38分，2024年国庆档(10月1日—10月7日)档期内预售总票房突破1亿元，《749局》《浴火之路》《志愿军：存亡之战》暂列档期预售票房榜前三位。";
        formData?.TryAdd("query", content);
        formData?.TryAdd("prompt_text", "确保已部署CosyVoice项目，已将 CosyVoice-api中的api.py放入，并成功启动了 api.py。");
        formData?.TryAdd("prompt_speech", "yy.wav");
        string jsonData = JsonConvert.SerializeObject(formData);
        string url = $"{TTSUrl}inference/streamclone";
        await PostTTSStream(url, jsonData, pcmData => {
            AudioClip audioClip = ConvertPCM16ToAudioClip(pcmData, 22050);
            onResult?.Invoke(audioClip);
        });
    }

    /// <summary>
    /// 获取TTS 的json流，使用base64编码
    /// </summary>
    /// <param name="content"></param>
    /// <param name="onResult"></param>
    /// <param name="speeker"></param>
    public async void GetTTSStream_SFT_Json(string content, Action<AudioClip> onResult, string speeker = null)
    {
        Dictionary<string, string> formData = new();
        formData?.TryAdd("query", content);
        formData?.TryAdd("speaker", speeker == null ? "中文女" : speeker);
        formData?.TryAdd("speed", 0.9f.ToString());
        formData?.TryAdd("isStream", false.ToString());
        string jsonData = JsonConvert.SerializeObject(formData);
        string url = $"{TTSUrl}inference/stream_sft_json";
        List<byte> totalPCMData = new();
        await PostTTSStream_Json(url, jsonData, pcmData => {
            AudioClip audioClip = ConvertPCM16ToAudioClip(pcmData, 22050);
            onResult?.Invoke(audioClip);
            totalPCMData.AddRange(pcmData);
        });
        string fileName = $"./audio/{DateTime.Now.ToString("yyyyMMddHHmmss")}" ;
        
        LLAudioConverter.SavePcmToFile(totalPCMData.ToArray(),  fileName + ".pcm");
        //根据时间起名字xxx.wav
        LLAudioConverter.SavePcmToWavFile(totalPCMData.ToArray(), fileName + ".wav");
    }

    /// <summary>
    /// 将PCM16转换为AudioClip
    /// </summary>
    /// <param name="pcmData"></param>
    /// <param name="sampleRate"></param>
    /// <returns></returns>
    public AudioClip ConvertPCM16ToAudioClip(byte[] pcmData, int sampleRate)
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



    /// <summary>
    /// Ollama 问答接口
    /// </summary>
    /// <param name="massage"></param>
    /// <param name="onResult"></param>
    /// <returns></returns>
    public async Task PostTTSStream(string apiURL, string jsonData, Action<byte[]> onResult)
    {
        //var packet = new Vector2();

        //string content = JsonConvert.SerializeObject(packet);
        using (HttpClient client = new HttpClient())
        {
            var request = new HttpRequestMessage(HttpMethod.Post, apiURL);
            request.Headers.Add("Accept", "application/octet-stream"); // 请求接收字节流
            request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            //超时5分钟
            //request.Headers.Add("timeout", "300000");
            client.Timeout = TimeSpan.FromMinutes(5);

            using (HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                using (var responseStream = await response.Content.ReadAsStreamAsync())
                {
                    int bufferSize = 22050 * 10;
                    byte[] buffer = new byte[bufferSize];
                    //List<byte> leftOver = new List<byte>();
                    byte[] leftOver = new byte[0];
                    int bytesRead;
                    int totalBytes = 0;
                    while (!IsDestroy && (bytesRead = await responseStream.ReadAsync(buffer, 0, bufferSize)) > 0)
                    {
                        byte[] chunk = new byte[bytesRead];
                        Buffer.BlockCopy(buffer, 0, chunk, 0, bytesRead);
                        var validBytes = CombineAndSeparateData(ref leftOver, chunk);
                        totalBytes += validBytes.Length;
                        Debug.LogError($"收到数据 length: {validBytes.Length}|{totalBytes}|{responseStream.CanRead}");
                        //onResult?.Invoke(chunk); // 处理每个字节流块
                        onResult?.Invoke(validBytes); // 处理每个字节流块
                    }
                    //Debug.LogError($"收取结束 length: {leftOver.Length}|{responseStream.CanRead}|{responseStream.CanSeek}");
                    //Debug.LogError($"收到数据 length: {totalBytes.Count}");

                }
            }
        }
    }

    public async Task PostTTSStream_Json(string apiURL, string jsonData, Action<byte[]> onResult)
    {
        using (HttpClient client = new HttpClient())
        {
            var request = new HttpRequestMessage(HttpMethod.Post, apiURL);
            request.Headers.Add("Accept", "text/event-stream");
            //request.Headers.Add("Authorization", "Bearer " + API_KEY);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            client.Timeout = TimeSpan.FromMinutes(15);
            using (HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            {
                ///确保请求成功，否则抛出异常
                response.EnsureSuccessStatusCode();

                using (var responseStream = await response.Content.ReadAsStreamAsync())
                using (var reader = new System.IO.StreamReader(responseStream))
                {
                    string request_id = response.Headers.GetValues("X-Request-ID").FirstOrDefault();
                    //缓存入request_id列表
                    if (!string.IsNullOrEmpty(request_id))RequestIdList.AddRequestID(request_id);
                    string line;
                    while (!IsDestroy && (line = await reader.ReadLineAsync()) != null)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            //{ "model":"qwen:7b","created_at":"2024-03-27T07:11:33.0059464Z","message":{ "role":"assistant","content":"\n"},"done":false}
                            LLTTSSteamDataBlock responesModelData = JsonConvert.DeserializeObject<LLTTSSteamDataBlock>(line);
                            var data = responesModelData.ToBytes();
                            Debug.LogError($"收到数据:{data.Length}|{request_id}");
                            onResult?.Invoke(data);
                        }
                    }
                    //移除request_id列表， 服务端自动结束，不用发送消息
                    RequestIdList.RemoveRequestID(request_id);
                }
            }
        }
    }




    /// <summary>
    /// 将剩余数据和当前数据合并，并返回剩余数据
    /// </summary>
    /// <param name="leftover"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static byte[] CombineAndSeparateData(ref byte[] leftover, byte[] value)
    {
        byte[] combinedValue = new byte[leftover.Length + value.Length];
        Array.Copy(leftover, combinedValue, leftover.Length);
        Array.Copy(value, 0, combinedValue, leftover.Length, value.Length);

        int byteLength = combinedValue.Length;
        int remainder = byteLength % 4;
        int validLength = byteLength - remainder;

        // Separate valid data and leftover
        byte[] validData = new byte[validLength];
        Array.Copy(combinedValue, validData, validLength);
        leftover = new byte[remainder];
        Array.Copy(combinedValue, validLength, leftover, 0, remainder);
        Debug.LogError("Valid Data: " + BitConverter.ToString(validData));
        Debug.LogError("New Leftover: " + BitConverter.ToString(leftover));
        // You can replace the Console.WriteLine statements with your logic to use validData and newLeftover
        return validData;
    }

    /// <summary>
    /// 发送停止生成请求
    /// </summary>
    /// <param name="url"></param>
    public void StopTTSGeneration()
    {
        string url = $"{TTSUrl}inference/stop_generation";
        StartCoroutine(SendStopGenerationRequest(url));
    }

    private IEnumerator SendStopGenerationRequest(string url)
    {
        if (RequestIdList == null || RequestIdList.request_ids.Count == 0) yield break;
        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            // 设置请求头
            www.SetRequestHeader("Content-Type", "application/json");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(RequestIdList.ToJson());
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            // 发送请求
            yield return www.SendWebRequest();

            if (www.isDone)
            {
                Debug.Log("Stop generation request successful.");
            }
            else
            {
                Debug.LogError("Error: " + www.error);
            }
        }
    }



    public AudioClip ConvertBinaryDataToAudioClip32(byte[] wavData)
    {
        float[] samples = new float[wavData.Length / 4]; // 32-bit audio, each sample is 4 bytes
        //float rescaleFactor = 2147483648.0f; // 32-bit audio, max value for int
        float rescaleFactor = int.MaxValue; // 32-bit audio, max value for int

        for (int i = 0; i < wavData.Length; i += 4)
        {
            int it = BitConverter.ToInt32(wavData, i); // Read 32-bit integer
            float ft = it / rescaleFactor;
            samples[i / 4] = ft;
        }

        AudioClip audioClip = AudioClip.Create("cosyvoice_tts", samples.Length, 1, 22050, false);
        audioClip.SetData(samples, 0);
        return audioClip;
    }

    public AudioClip ConvertBinaryDataToAudioClip(byte[] wavData)
    {
        float[] samples = new float[wavData.Length / 2];
        float rescaleFactor = 32768.0f; // 16-bit audio
        short st = 0;
        float ft = 0;
        for (int i = 0; i < wavData.Length; i+= 2)
        {
            st = BitConverter.ToInt16(wavData, i);
            ft = st / rescaleFactor;
            samples[i / 2] = ft;
        }
        AudioClip audioClip = AudioClip.Create("cosyvoice_tts", samples.Length, 1, 22050, false);
        audioClip.SetData(samples, 0);
        return audioClip;

    }
}


public class LLTTSSteamDataBlock
{
    public string data;

    //base64编码data转为byte[]
    public byte[] ToBytes()
    {
        return System.Convert.FromBase64String(data);
    }
}

/// <summary>
/// TTS请求生成音频数据块
/// </summary>
public class LLTTSRequestStopGenerationBlock {
    public List<string> request_ids = new();

    public void Clear()
    {
        request_ids.Clear();
    }

    public void AddRequestID(string request_id)
    {
        request_ids.Add(request_id);
    }

    /// <summary>
    /// remove request_id
    /// </summary>
    /// <param name="request_id"></param>
    /// <returns></returns>
    public bool RemoveRequestID(string request_id)
    {
        return request_ids.RemoveAll((item) => item == request_id) > 0;
    }

    ///<summary>
    /// json格式化
    /// </summary>
    /// <returns></returns>
    public string ToJson()
    {
        return JsonConvert.SerializeObject(this);
    }
}
