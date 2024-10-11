using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using LLVoice.Tools;

namespace SG.AI
{
    public class SGOllamaNet : MonoSingleton<SGOllamaNet>
    {
        //dify apikey
        public static string API_KEY = "app-408X8F3pYzqWd5kG7jT7lq6z";//app-408X8F3pYzqWd5kG7jT7lq6z
        public static Queue<SGOllamaNet_Dify_REQ_Body> rspQueue = new Queue<SGOllamaNet_Dify_REQ_Body>();
        [SerializeField]
        string apiURL = "http://127.0.0.1:11434/api/chat";
        public string modelName = "qwen:7b";
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
        /// Ollama 问答接口
        /// </summary>
        /// <param name="massage"></param>
        /// <param name="onResult"></param>
        /// <returns></returns>
        public async Task OllamaAnswer(string massage, Action<string> onResult, bool isStream = true)
        {
            Ollama_PacketModelData packet = new Ollama_PacketModelData()
            {
                model = modelName,
                messages = new List<Ollama_PacketMassage>()
            {
                new Ollama_PacketMassage()
                {
                    role = "user",
                    content = massage,
                }
            },

                stream = isStream,
            };

            //string url = "http://localhost:8080/sse_endpoint"; // SSE endpoint URL
            string content = JsonConvert.SerializeObject(packet);
            using (HttpClient client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, apiURL);
                request.Headers.Add("Accept", "text/event-stream");
                //request.Headers.Add("Authorization", "Bearer " + API_KEY);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Content = new StringContent(content, Encoding.UTF8, "text/event-stream");

                using (HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                {
                    ///确保请求成功，否则抛出异常
                    response.EnsureSuccessStatusCode();

                    using (var responseStream = await response.Content.ReadAsStreamAsync())
                    using (var reader = new System.IO.StreamReader(responseStream))
                    {
                        string line;
                        while (!IsDestroy &&(line = await reader.ReadLineAsync()) != null)
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                //{ "model":"qwen:7b","created_at":"2024-03-27T07:11:33.0059464Z","message":{ "role":"assistant","content":"\n"},"done":false}
                                Ollama_StreamedPacketModelData responesModelData = JsonConvert.DeserializeObject<Ollama_StreamedPacketModelData>(line);
                                //await UniTask.SwitchToMainThread(); // 切换到主线程更新UI
                                onResult?.Invoke(responesModelData.message.content);
                                // Handle the received SSE event here
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 请求数据
    /// </summary>
    public class SGOllamaNet_Dify_REQ_Body {
        public Dictionary<string, string> inputs = new Dictionary<string, string>();
        /// <summary>
        /// 提问内容
        /// </summary>
        public string query;
        public string response_mode = "streaming";
        /// <summary>
        /// 会话 ID
        /// </summary>
        public string conversation_id;
        /// <summary>
        /// 唯一ID
        /// </summary>
        public string user;
        public string[] files;

        public string ToJson() { 
            return JsonConvert.SerializeObject(this);
        }
    }

    /// <summary>
    /// 回复数据
    /// </summary>
    //data: {"event": "message", "task_id": "900bbd43-dc0b-4383-a372-aa6e6c414227", "id": "663c5084-a254-4040-8ad3-51f2a3c1a77c", "answer": "Hi", "created_at": 1705398420}\n\n
    public class SGOllamaNet_Dify_RSP_Body
    {
        /// <summary>
        /// 提问内容
        /// </summary>
        //public string event;
        public string task_id = "streaming";
        /// <summary>
        /// 会话 ID
        /// </summary>
        public string id;
        /// <summary>
        /// 唯一ID
        /// </summary>
        public string answer;
        public string created_at;

        public static SGOllamaNet_Dify_RSP_Body GetBody(string json) {
            return JsonConvert.DeserializeObject<SGOllamaNet_Dify_RSP_Body>(json);
        }
    }

    [Serializable]
    public class Ollama_PacketMassage
    {
        public string role;
        public string content;
    }

    [Serializable]
    public class Ollama_PacketModelData
    {
        public string model;
        public List<Ollama_PacketMassage> messages;
        public bool stream;
    }

    [Serializable]
    public class Ollama_StreamedPacketModelData
    {
        public string model;
        public string created_at;
        public Ollama_PacketMassage message;
        public bool done;
    }

    [Serializable]
    public class Ollama_ResponesMessage
    {
        public string role;
        public string content;
    }

    /// <summary>
    /// Ollama返回数据
    /// </summary>
    [Serializable]
    public class Ollama_ResponesModelData
    {
        public string model;
        public DateTime created_at;
        public Ollama_ResponesMessage message;
        public bool done;
        public long total_duration;
        public long load_duration;
        public int prompt_eval_count;
        public long prompt_eval_duration;
        public int eval_count;
        public long eval_duration;
    }
}