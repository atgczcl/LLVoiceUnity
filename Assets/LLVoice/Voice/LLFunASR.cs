using LLVoice.Net;
using LLVoice.Tools;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;


namespace LLVoice.Voice
{
    public class LLFunASR : MonoSingleton<LLFunASR>
    {
        /// <summary>
        /// 表示流式模型latency配置，`[5, 10, 5]`，表示当前音频为600ms，并且回看300ms，又看300ms。
        /// </summary>
        public int[] chunk_size = new int[] { 5, 10, 5 };
        /// <summary>
        /// 
        /// </summary>
        public int chunk_interval = 10;
        public MessageHandler msgHandler = new();
        public UnityEvent<string> OnMessageCallback;
        public string websocketUrl = "ws://127.0.0.1:10096/";
        public string websocketKey = "LLFunASR-websocket";

        public LLWebSocket websocket;

        private void Start()
        {
            //测试输出
            msgHandler.OnMessageCallback = OnMessageCallback;
            Debug.Log("websocket start test");
            websocket = LLWebSocketManager.Instance.AddWebSocket(websocketKey, websocketUrl, onConnect: () => {
                Debug.Log("开始初始化");
                //默认已经进行了切回主线程处理
                Init();
            }, onStrMsg:OnMessage);

        }

        private void OnMessage(string msg)
        {
            Debug.Log("websocket 收到消息: " + msg);
            msgHandler.ReceiveMessages(msg);
            //OnMessageCallback?.Invoke(msg);
        }

        public void Init()
        {
            //初始化
            ClientFirstConnOnline();
            Debug.Log("websocket 初始化完成");
            LLMicrophoneRecorderMgr.Instance.Initialized();
            //异步线程无法启动协程
            //StartCoroutine(test());
        }

        //IEnumerator test()
        //{
        //    Debug.LogError("测试，测试GG111");
        //    yield return new WaitForSeconds(3);
        //    Debug.LogError("测试，测试GG222");
        //}

        /// <summary>
        /// 客户端首次连接消息
        /// </summary>
        /// <param name="asrmode">online, offline, 2pass</param>
        /// <returns></returns>
        public bool ClientFirstConnOnline(string asrmode = "online")
        {
            // 参数说明：
            // `mode`：`offline`，表示推理模式为一句话识别；`online`，表示推理模式为实时语音识别；`2pass`：表示为实时语音识别，并且说话句尾采用离线模型进行纠错。
            //`wav_name`：表示需要推理音频文件名
            //`wav_format`：表示音视频文件后缀名，只支持pcm音频流
            //`is_speaking`：表示断句尾点，例如，vad切割点，或者一条wav结束
            //`chunk_size`：表示流式模型latency配置，`[5, 10, 5]`，表示当前音频为600ms，并且回看300ms，又看300ms。
            //`audio_fs`：当输入音频为pcm数据是，需要加上音频采样率参数
            //`hotwords`：如果使用热词，需要向服务端发送热词数据（字符串），格式为 "{"阿里巴巴":20,"通义实验室":30}"
            //`itn`: 设置是否使用itn，默认True
            // jsonresult={"chunk_interval":10,"chunk_size":[5,10,5],"hotwords":"{\"\\u4f60\\u597d\": 20, \"\\u67e5\\u8be2\": 30}","is_speaking":true,"itn":true,"mode":"2pass","wav_name":"microphone"}, msg_data->msg={"access_num":0,"audio_fs":16000,"is_eof":false,"itn":true,"mode":"2pass","wav_format":"pcm","wav_name":"microphone"}

            //string hotwords = "{\'你好\':20,\'查询\':30}";
            //string firstbuff = $"{{\"mode\": \"{asrmode}\", \"chunk_size\": [{chunk_size[0]},{chunk_size[1]},{chunk_size[2]}], \"chunk_interval\": {chunk_interval},\"hotwords\": \"{hotwords}\", \"wav_name\": \"microphone\", \"is_speaking\": true, \"itn\":false}}";
            //LLWebSocket.Instance.Send(firstbuff);
            //ClientSendAudioFunc(firstbuff);



            var hotwords = new Dictionary<string, int>
            {
                {"你好", 20},
                {"查询", 30}
            };

            var jsonResult = new
            {
                chunk_interval = 10,
                chunk_size = new List<int> { 5, 10, 5 },
                hotwords = JsonConvert.SerializeObject(hotwords),
                is_speaking = true,
                itn = false,
                mode = asrmode,
                wav_name = "microphone"
            };

            string jsonString = JsonConvert.SerializeObject(jsonResult);
            Debug.Log("Config jsonString: " + jsonString);
            LLWebSocketManager.Instance.Send(websocketKey, jsonString);
            return true;
        }

        public bool ClientSendAudioFunc(byte[] buff)    //实时识别
        {
            int wave_buffer_collectfrequency = 16000;
            ////发送音频数据
            int CHUNK = wave_buffer_collectfrequency / 1000 * 60 * chunk_size[1] / chunk_interval;
            for (int i = 0; i < buff.Length; i += CHUNK)
            {
                byte[] send = buff.Skip(i).Take(CHUNK).ToArray();
                //Task.Run(() => client.Send(send));
                //Thread.Sleep(1);
                websocket.Send(send);
            }

            return true;
        }

    }




    public class MessageHandler
    {
        private string onlineText = "";
        //public Queue<LLFunMessage> messageQueue = new Queue<LLFunMessage>();
        public UnityEvent<string> OnMessageCallback;

        public void ReceiveMessages(string message)
        {
            //var meg = JsonUtility.FromJson<LLFunMessage>(message);
            //HandleText(meg);

            GetJsonMessage(message);
        }

        public string offline_text = "";
        public string rec_text = "";
        public string online_text = "";
        // Method to handle the message
        public void GetJsonMessage(string jsonMsg)
        {
            var data = JsonUtility.FromJson<LLFunMessage>(jsonMsg);
            Debug.Log("message: " + data.text);
            string rectxt = data.text;
            string asrmodel = data.mode;
            bool is_final = data.is_final;
            var timestamp = data.timestamp;

            if (asrmodel == "2pass-offline" || asrmodel == "offline")
            {
                offline_text += HandleWithTimestamp(rectxt, timestamp);
                rec_text = offline_text;
            }
            else
            {
                online_text += rectxt;
                rec_text += rectxt;
            }

            //varArea.text = rec_text;

            //Debug.Log("offline_text: " + asrmodel + "," + offline_text);
            //Debug.Log("rec_text: " + rec_text);
            UpdateConsoleText(online_text);
        }

        /// <summary>
        /// Method to handle the message with timestamps
        /// </summary>
        /// <param name="tmptext"></param>
        /// <param name="tmptime"></param>
        /// <returns></returns>
        public string HandleWithTimestamp(string tmptext, List<List<long>> tmptime)
        {
            //Debug.Log("tmptext: " + tmptext);
            //Debug.Log("tmptime: " + tmptime);

            if (tmptime == null || tmptime.Count == 0 || string.IsNullOrEmpty(tmptext))
            {
                return tmptext;
            }

            tmptext = Regex.Replace(tmptext, @"[。？，、\?\. ]", ",");
            var words = tmptext.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            int char_index = 0;
            var text_withtime = new StringBuilder();

            foreach (var word in words)
            {
                if (string.IsNullOrEmpty(word))
                {
                    continue;
                }

                Debug.Log("words===" + word);
                Debug.Log("words: " + word + ",time=" + tmptime[char_index][0] / 1000f);

                if (Regex.IsMatch(word, @"^[a-zA-Z]+$"))
                {
                    text_withtime.Append(tmptime[char_index][0] / 1000f + ":" + word + "\n");
                    char_index++;
                }
                else
                {
                    text_withtime.Append(tmptime[char_index][0] / 1000f + ":" + word + "\n");
                    char_index += word.Length;
                }
            }

            return text_withtime.ToString();
        }

        private string recbuff = string.Empty;//接收累计缓存内容
        public void HandleText(LLFunMessage meg)
        {
            if (meg.mode == "2pass-online")
            {
                onlineText += meg.text;
                UpdateConsoleText($"{recbuff}{onlineText}");
                //UpdateConsoleText($"{textPrint2PassOnline}");
                //Console.WriteLine(recbuff + onlinebuff);
            }
            else if (meg.mode == "2pass-offline")
            {
                recbuff += meg.text;
                onlineText = string.Empty;
                //Console.WriteLine(recbuff);
            }

            if (meg.is_final)//未结束当前识别
            {
                recbuff = string.Empty;
            }
        }

        private void UpdateConsoleText(string text)
        {
            // 更新控制台文本
            Debug.Log($"识别结果: {text}");
            OnMessageCallback?.Invoke(text);
        }

        // 假设这些属性已经定义好
        public int WordsMaxPrint { get; set; }
    }


    [System.Serializable]
    public class LLFunMessage
    {
        public bool is_final;
        public string mode;
        public List<LLFunStampSent> stamp_sents;
        public string text;
        public List<List<long>> timestamp;
        public string wav_name;
    }

    [System.Serializable]
    public class LLFunStampSent
    {
        public long end;
        public string punc;
        public long start;
        public string text_seg;
        public List<List<long>> ts_list;
    }
}
