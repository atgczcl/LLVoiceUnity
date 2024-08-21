using LLStar.Net;
using LLVoice.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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


        private void Start()
        {
            msgHandler.OnMessageCallback = OnMessageCallback;
            Debug.Log("websocket start test");
            LLWebSocket.Instance.Connect(onConnect: () => {
                Init();
                LLMicrophoneRecorderMgr.Instance.Initialized();
            }, onMessageCallback: OnMessage);
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
        }

        private void Update()
        {
            if (msgHandler != null) msgHandler.DispatchMessages();
        }

        /// <summary>
        /// 客户端首次连接消息
        /// </summary>
        /// <param name="asrmode">online, offline, 2pass</param>
        /// <returns></returns>
        public bool ClientFirstConnOnline(string asrmode = "2pass")
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
            string hotwords = "{{\'你好\':20,\'查询\':30}}";
            //string hotwords = "阿里巴巴 20\n达摩院 20\n夜雨飘零 20\n";
            string firstbuff = $"{{\"mode\": \"{asrmode}\", \"chunk_size\": [{chunk_size[0]},{chunk_size[1]},{chunk_size[2]}], \"chunk_interval\": {chunk_interval},\"hotwords\": \"{hotwords}\", \"wav_name\": \"microphone\", \"is_speaking\": true}}";
                       //, asrmode, chunk_size[0], chunk_size[1], chunk_size[2], chunk_interval, hotwords);
            LLWebSocket.Instance.Send(firstbuff);
            //ClientSendAudioFunc(firstbuff);
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
                LLWebSocket.Instance.Send(send);
            }

            return true;
        }

    }




    public class MessageHandler
    {
        private bool offlineMsgDone;
        private string textPrint="";
        private string textPrint2PassOnline = "";
        private string textPrint2PassOffline = "";
        private int id = 0;
        private int wordsMaxPrint = 1000;

        public Queue<LLFunMessage> messageQueue = new Queue<LLFunMessage>();

        public UnityEvent<string> OnMessageCallback;

        public void ReceiveMessages(string message)
        {
            //string message = ReceiveWebSocketMessage();
            var meg = JsonUtility.FromJson<LLFunMessage>(message);
            //string text = meg.text;
            //bool isFinal = meg.is_final;
            //List<List<long>> timestamp = meg.timestamp;

            //HandleText(meg, text, timestamp, isFinal);
            messageQueue.Enqueue(meg);

        }

        /// <summary>
        /// 分发消息
        /// </summary>
        public void DispatchMessages()
        {
            while (messageQueue.Count > 0)
            {
                var meg = messageQueue.Dequeue();
                //string text = meg.text;
                //bool isFinal = meg.is_final;
                //List<List<long>> timestamp = meg.timestamp;
                HandleText(meg);
            }
        }


        private string recbuff = string.Empty;//接收累计缓存内容
        public void HandleText(LLFunMessage meg)
        {
            //if (meg.mode == "online")
            //{
            //    textPrint += text;
            //    //textPrint = textPrint.Substring(textPrint.Length - wordsMaxPrint);
            //    UpdateConsoleText(textPrint);
            //}
            //else if (meg.mode == "offline")
            //{
            //    if (timestamp != null&& timestamp.Count > 0)
            //    {
            //        textPrint += $"{text} timestamp: {timestamp}";
            //    }
            //    else
            //    {
            //        textPrint += text;
            //    }

            //    UpdateConsoleText($"Offline: {textPrint}");
            //    offlineMsgDone = true;
            //}
            //else {
            //    if (meg.mode == "2pass-online")
            //    {
            //        textPrint2PassOnline += text;
            //        textPrint = textPrint2PassOffline + textPrint2PassOnline;
            //    }
            //    else
            //    {
            //        textPrint2PassOnline = "";
            //        textPrint = textPrint2PassOffline + text;
            //        textPrint2PassOffline += text;
            //    }
            //    //textPrint = textPrint.Substring(textPrint.Length - wordsMaxPrint);
            //    UpdateConsoleText($"\r{textPrint}");
            //}


            if (meg.mode == "2pass-online")
            {
                textPrint2PassOnline += meg.text;
                UpdateConsoleText($"{recbuff}{textPrint2PassOnline}");
                //UpdateConsoleText($"{textPrint2PassOnline}");
                //Console.WriteLine(recbuff + onlinebuff);
            }
            else if (meg.mode == "2pass-offline")
            {
                recbuff += meg.text;
                textPrint2PassOnline = string.Empty;
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
