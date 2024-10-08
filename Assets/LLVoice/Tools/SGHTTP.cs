using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json;
using LLVoice.Tools;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.XR;
using System.Linq;

namespace cscec.IOC
{
    /// <summary>
    /// Http请求
    /// </summary>
    public class SGHTTP : MonoSingleton<SGHTTP>
    {
        public static bool IsDestroy = false;

        public override void Awake()
        {
            base.Awake();
            IsDestroy = false;
        }

        private void OnDestroy()
        {
            IsDestroy = true;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Initialize()
        {
            if (Instance == null)
            {
                //Instance = new GameObject("SGHTTP").AddComponent<SGHTTP>();
                //DontDestroyOnLoad(Instance);
                Debug.LogError($"SGHTTP Instance is null! error!");
            }
        }

        /// <summary>
        /// POST 请求数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="complete"></param>
        /// <param name="formData"></param>
        /// <returns></returns>
        public static Coroutine PostRequest(string url, Action<DownloadHandler> complete, Dictionary<string, string> formData)
        {
            Initialize();
            return Instance.StartCoroutine(PostWebRequest(url, complete, formData));
        }

        /// <summary>
        /// 协成 POST 上传json数据
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        static IEnumerator PostWebRequest(string url, Action<DownloadHandler> complete, Dictionary<string, string> formData)
        {
            string postJson = JsonConvert.SerializeObject(formData);
            byte[] postBytes = System.Text.Encoding.UTF8.GetBytes(postJson);
            using UnityWebRequest webRequest =new(url, "POST");
            webRequest.uploadHandler = new UploadHandlerRaw(postBytes);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.certificateHandler = new WebReqSkipCert();
            //application/x-www-form-urlencoded
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Access-Control-Allow-Origin", "*");
            webRequest.SetRequestHeader("accept", "text/plain");
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.isDone)
            {
                Debug.Log($"{url}:{postJson}: " + webRequest.downloadHandler.text);
                complete?.Invoke(webRequest.downloadHandler);
            }
            else
            {
                Debug.Log($"ReadStreamFile Error:" + webRequest.error);
            }
            webRequest.Dispose();
        }

        /// <summary>
        /// 公共信息请求接口，登录成功后可用，自动传token
        /// </summary>
        /// <param name="url"></param>
        /// <param name="complete"></param>
        /// <param name="postJson"></param>
        /// <returns></returns>
        public static Coroutine PostCommonRequest(string url, Action<DownloadHandler> complete, string postJson)
        {
            Initialize();
            return Instance.StartCoroutine(PostCommonWebRequest(url, complete, postJson));
        }

        /// <summary>
        /// 协成 POST 上传json数据
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        static IEnumerator PostCommonWebRequest(string url, Action<DownloadHandler> complete, string postJson)
        {
            //string postJson = LitJson.JsonMapper.ToJson(formData);
            byte[] postBytes = System.Text.Encoding.UTF8.GetBytes(postJson);
            using UnityWebRequest webRequest = new(url, "POST");
            webRequest.uploadHandler = new UploadHandlerRaw(postBytes);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.certificateHandler = new WebReqSkipCert();
            //application/x-www-form-urlencoded
            //Debug.LogError("Authorization:" + LoginCtrol.access_token);
            //webRequest.SetRequestHeader("Authorization", "Bearer " + LoginCtrol.access_token);
            //webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Content-Type", "application/json-patch+json");
            webRequest.SetRequestHeader("Access-Control-Allow-Origin", "*");
            webRequest.SetRequestHeader("accept", "text/plain");
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.isDone)
            {
                Debug.Log($"POST:{url}:{postJson}: " + webRequest.downloadHandler.text);
                complete?.Invoke(webRequest.downloadHandler);
            }
            else
            {
                Debug.Log($"ReadStreamFile Error:" + webRequest.error);
            }
            webRequest.Dispose();
        }

        /// <summary>
        /// 合成语音
        /// </summary>
        /// <param name="url">http://127.0.0.1:8080/v1/tts</param>
        /// <param name="text"></param>
        /// <param name="speaker">中文女,中文男,英文女,英文男,日语男,粤语女,韩语女</param>
        /// <param name="complete"></param>
        /// <returns></returns>
        public static Coroutine SendTTSRequest(string url, string text, string speaker, Action<AudioClip> complete)
        { 
            Initialize();
            return Instance.StartCoroutine(OnSendTTSRequest(url, text, speaker, complete));
        }

        public static IEnumerator OnSendTTSRequest(string url, string text, string speaker, Action<AudioClip> complete)
        {
            var wwwForm = new WWWForm();
            wwwForm.AddField("text", text);
            wwwForm.AddField("spk", speaker);

            using (var request = UnityWebRequestMultimedia.GetAudioClip($"http://1.94.131.28:19463/tts?text={text}&spk={speaker}", AudioType.WAV))
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

        public static async void GetTTSStream(Action<AudioClip> onResult) {
            Dictionary<string, string> formData = new();
            //  {
            //      query: data,
            //  prompt_text: '确保已部署CosyVoice项目，已将 CosyVoice-api中的api.py放入，并成功启动了 api.py。',
            //  prompt_speech: "yy.wav"
            //}
//            //string cc = @"中新网北京9月29日电 (记者 高凯)据灯塔专业版数据，截至9月29日14时38分，2024年国庆档(10月1日—10月7日)档期内预售总票房突破1亿元，《749局》《浴火之路》《志愿军：存亡之战》暂列档期预售票房榜前三位。
//根据灯塔专业版上映日历显示，今年国庆档将有10部新片上映，涵盖剧情、战争、犯罪、动作、灾难、家庭、科幻、冒险、青春、历史、歌舞、动画、喜剧、奇幻等多种类型，其中真人影片8部，不仅在数量上比去年国庆档多2部，并且都是中上制作规模的影片，也是近几年头部及中腰部影片数量最多的一次国庆档。";
            string cc = @"中新网北京9月29日电 (记者 高凯)据灯塔专业版数据，截至9月29日14时38分，2024年国庆档(10月1日—10月7日)档期内预售总票房突破1亿元，《749局》《浴火之路》《志愿军：存亡之战》暂列档期预售票房榜前三位。";
            formData?.TryAdd("query", cc);
            formData?.TryAdd("prompt_text", "确保已部署CosyVoice项目，已将 CosyVoice-api中的api.py放入，并成功启动了 api.py。");
            formData?.TryAdd("prompt_speech", "yy.wav");
            string jsonData = JsonConvert.SerializeObject(formData);
            string url = "http://127.0.0.1:8080//inference/stream";
            await PostTTSStream(url, jsonData, pcmData => { 
                AudioClip audioClip = ConvertPCM16ToAudioClip(pcmData, 22050);
                onResult?.Invoke(audioClip);
            });
        }

        public static AudioClip ConvertPCMToAudioClip(byte[] pcmData, int sampleRate, int channels)
        {
            // 计算数据长度
            int numSamples = pcmData.Length / (channels * 2); // 每个样本占用2字节（16位）

            // 创建AudioClip
            AudioClip audioClip = AudioClip.Create("GeneratedAudioClip", numSamples, channels, sampleRate, false);

            // 将PCM数据转换为浮点数数组
            float[] samples = new float[numSamples * channels];
            for (int i = 0; i < numSamples * channels; i++)
            {
                short sampleValue = (short)(BitConverter.ToInt16(pcmData, i * 2) / 32768.0f); // 归一化到 -1.0 到 1.0
                samples[i] = sampleValue;
            }

            // 设置音频数据
            audioClip.SetData(samples, 0);

            return audioClip;
        }
        // 将16位PCM字节数组转换为AudioClip
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



        /// <summary>
        /// Ollama 问答接口
        /// </summary>
        /// <param name="massage"></param>
        /// <param name="onResult"></param>
        /// <returns></returns>
        public static async Task PostTTSStream(string apiURL, string jsonData, Action<byte[]> onResult)
        {
            //var packet = new Vector2();

            //string content = JsonConvert.SerializeObject(packet);
            using (HttpClient client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, apiURL);
                request.Headers.Add("Accept", "application/octet-stream"); // 请求接收字节流
                request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                using (HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    using (var responseStream = await response.Content.ReadAsStreamAsync())
                    {
                        int bufferSize = 22050*10;
                        byte[] buffer = new byte[bufferSize];
                        List<byte> totalBytes = new List<byte>();
                        int bytesRead;
                        while (!IsDestroy && (bytesRead = await responseStream.ReadAsync(buffer, 0, bufferSize)) > 0)
                        {
                            byte[] chunk = new byte[bytesRead];
                            Buffer.BlockCopy(buffer, 0, chunk, 0, bytesRead);
                            totalBytes.AddRange(chunk);
                            Debug.LogError($"收到数据 length: {chunk.Length}|{responseStream.CanRead}");
                            //onResult?.Invoke(chunk); // 处理每个字节流块
                            //totalBytes长度超过5秒，则处理
                            if (totalBytes.Count % 4 == 0)
                            {
                                onResult?.Invoke(totalBytes.ToArray()); // 处理每个字节流块
                                totalBytes.Clear();
                            }
                        }
                        Debug.LogError($"收取结束 length: {totalBytes.Count}|{responseStream.CanRead}|{responseStream.CanSeek}");
                        //Debug.LogError($"收到数据 length: {totalBytes.Count}");
                        if (totalBytes.Count > 0)
                        {
                            //结束播放剩余的数据
                            onResult?.Invoke(totalBytes.ToArray()); // 处理每个字节流块
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 下载pcm流
        /// </summary>
        /// <param name="url"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        public static Coroutine PostPCMRequest(string url, Action<byte[]> complete)
        {
            Initialize();
            Dictionary<string, string> formData = new();
            //  {
            //      query: data,
            //  prompt_text: '确保已部署CosyVoice项目，已将 CosyVoice-api中的api.py放入，并成功启动了 api.py。',
            //  prompt_speech: "yy.wav"
            //}
            formData?.TryAdd("query", "床前明月光，疑是地上霜。");
            formData?.TryAdd("prompt_text", "确保已部署CosyVoice项目，已将 CosyVoice-api中的api.py放入，并成功启动了 api.py。");
            formData?.TryAdd("prompt_speech", "yy.wav");
            string jsonData = JsonConvert.SerializeObject(formData);
            var co = Instance.StartCoroutine(PostStreamRequest(url, handler => {
                byte[] bytes = handler.data;
            }, jsonData));
            return co;
        }

        static IEnumerator PostStreamRequest(string url, Action<DownloadHandler> complete, string postJson)
        {
            //string postJson = LitJson.JsonMapper.ToJson(formData);
            byte[] postBytes = System.Text.Encoding.UTF8.GetBytes(postJson);
            using UnityWebRequest webRequest = new(url, "POST");
            webRequest.uploadHandler = new UploadHandlerRaw(postBytes);
            var handler = new DownloadHandlerBuffer();
            webRequest.downloadHandler = handler;
            webRequest.certificateHandler = new WebReqSkipCert();
            //application/x-www-form-urlencoded
            //Debug.LogError("Authorization:" + LoginCtrol.access_token);
            //webRequest.SetRequestHeader("Authorization", "Bearer " + LoginCtrol.access_token);
            //webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Content-Type", "application/json-patch+json");
            webRequest.SetRequestHeader("Access-Control-Allow-Origin", "*");
            webRequest.SetRequestHeader("accept", "text/plain");
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.isDone)
            {
                Debug.Log($"POST:{url}:{postJson}: " + webRequest.downloadHandler.text);
                complete?.Invoke(webRequest.downloadHandler);
            }
            else
            {
                Debug.Log($"ReadStreamFile Error:" + webRequest.error);
            }
            webRequest.Dispose();
        }


        /// <summary>
        /// 公共查询自动填写token信息
        /// </summary>
        /// <param name="url"></param>
        /// <param name="complete"></param>
        /// <param name="formData"></param>
        /// <returns></returns>
        public static Coroutine GetCommonRequest(string url, Action<DownloadHandler> complete, Dictionary<string, string> formData = null)
        {
            Initialize();
            if (formData == null)
            {
                formData = new();
            }
            //formData?.TryAdd("Authorization", "Bearer " + LoginCtrol.access_token);
            return Instance.StartCoroutine(GetWebRequest(url, formData, complete));
        }

        /// <summary>
        /// GET 请求数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="complete"></param>
        /// <param name="formData"></param>
        /// <returns></returns>
        public static Coroutine GetRequest(string url, Action<DownloadHandler> complete, Dictionary<string, string> formData)
        {
            Initialize();
            return Instance.StartCoroutine(GetWebRequest(url, formData, complete));
        }

        /// <summary>
        /// 协成 GET 请求数据
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="complete"></param>
        /// <param name="formData">中文不好处理，暂时不支持数据传输</param>
        /// <returns></returns>
        static IEnumerator GetWebRequest(string url, Dictionary<string, string> formData, Action<DownloadHandler> complete)
        {
            using UnityWebRequest webRequest = UnityWebRequest.Get(url);
            webRequest.certificateHandler = new WebReqSkipCert();
            webRequest.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            foreach (var item in formData)
            {
                //Debug.Log($"AddFormData:{item.Key}:{item.Value}");
                webRequest.SetRequestHeader(item.Key, item.Value);
            }
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.isDone)
            {
                Debug.Log($"GET:WebRequest:{webRequest.downloadHandler.text}|{url}:");
                complete?.Invoke(webRequest.downloadHandler);
            }
            else
            {
                Debug.Log($"ReadStreamFile Error:" + webRequest.error);
            }
            webRequest.Dispose();
        }
    }

    /// <summary>
    /// 避免https检测证书
    /// </summary>
    public class WebReqSkipCert : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }

}