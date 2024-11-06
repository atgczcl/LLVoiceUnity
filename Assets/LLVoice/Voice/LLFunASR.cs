using cscec.IOC;
using LLVoice.Net;
using LLVoice.Tools;
using Newtonsoft.Json;
using SG.AI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
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
        public FunASR_MessageHandler msgHandler;
        public UnityEvent<string> OnResultEvent = new();
        public string websocketUrl = "ws://127.0.0.1:10096/";
        public string websocketKey = "LLFunASR-websocket";

        public LLWebSocket websocket;
        public AudioSource audioSource;
        //播放队列
        public LLAudioPlayQueue audioPlayQueue;
        public string testTTSString = @"中新网北京9月29日电 (记者 高凯)据灯塔专业版数据，截至9月29日14时38分，2024年国庆档(10月1日—10月7日)档期内预售总票房突破1亿元，《749局》《浴火之路》《志愿军：存亡之战》暂列档期预售票房榜前三位。";

        public override void Awake()
        {
            base.Awake();
            if (!audioSource)
                audioSource = gameObject.GetOrAddComponent<AudioSource>();
            audioPlayQueue = gameObject.GetOrAddComponent<LLAudioPlayQueue>();
            audioPlayQueue.audioSource = audioSource;
        }

        private void Start()
        {
            Debug.Log("Websocket starting...");
            websocket = LLWebSocketManager.Instance.AddWebSocket(websocketKey, websocketUrl, onConnect: () => {
                Debug.Log("开始初始化");
                //默认已经进行了切回主线程处理
                Init();
            }, onStrMsg:OnMessage);
            //TestTTS();
            //SendChatRequest(testTTSString);
        }

        /// <summary>
        /// 发送聊天请求
        /// </summary>
        public async void SendChatRequest(string text)
        {
            await SGOllamaNet.Instance.OllamaAnswer(text, answer =>
            {
                Debug.Log("收到回复: " + answer);
                SendTTS(answer);
            }, false);
        }

        /// <summary>
        /// TTS http post 请求测试
        /// </summary>
        public void SendTTS(string content)
        {
            //curl -X POST   "http://127.0.0.1:8080/v1/tts"   -F "text=确保已部署CosyVoice项目，已将 CosyVoice-api中的api.py放入，并成功启动了 api.py。"   -F "spk=中文女"   --output output.wav
            //var url = "http://127.0.0.1:8080/v1/tts";
            //var text = "确保已部署CosyVoice项目，已将 CosyVoice-api中的api.py放入，并成功启动了 api.py。";
            //var spk = "中文女";

            //LLTTSManager.Instance.SendTTSRequest(url, text, spk, handler =>
            //{
            //    //SGHTTP.SendTTSRequest(url, text, spk, handler => {
            //    Debug.Log("TTS 请求成功");
            //    audioSource.clip = handler;
            //    audioSource.Play();
            //}, true);

            LLTTSManager.Instance.GetTTSStream_SFT_Json(content, clip => { 
                Debug.Log("TTS 请求成功");
                if (clip != null) {
                    audioPlayQueue.Enqueue(clip);
                }
            });
        }

        private void OnMessage(string msg)
        {
            Debug.Log("websocket 收到消息: " + msg);
            msgHandler.ReceiveMessages(msg);
            //OnMessageCallback?.Invoke(msg);
        }

        public void Init()
        {
            if (!TryGetComponent(out msgHandler))
            {
                msgHandler = gameObject.AddComponent<FunASR_MessageHandler>();
            }
            //测试输出
            msgHandler.OnRecogniseCallback.AddListener(OnResultCallbacks);
            msgHandler.OnIsSpeakingCallback = SendIsSpeaking;
            msgHandler.OnIdleLongTimeCallback = () => { SendIsSpeaking(false); };

            //初始化
            ClientFirstConnOnline();
            Debug.Log("websocket 初始化完成");
            LLMicrophoneRecorderMgr.Instance.Initialized();
            //异步线程无法启动协程
            //StartCoroutine(test());
        }

        /// <summary>
        /// OnRecogniseCallback
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public void OnResultCallbacks(string result)
        {
            Debug.Log("OnResultCallback: " + result);
            if (string.IsNullOrEmpty(result)) { 
                return;
            }

            OnResultEvent?.Invoke(result);
            SendChatRequest(result);
        }

        ///<summary>
        ///发送说话状态
        ///</summary>
        ///<param name="isSpeaking"></param>
        ///<returns></returns>
        public void SendIsSpeaking(bool isSpeaking)
        {
            Debug.Log("isSpeaking:" + isSpeaking);
            var data = new LLFunSpeakingState { is_speaking = isSpeaking };
            var json = JsonUtility.ToJson(data);
            //Debug.Log("SendIsSpeaking: " + json);
            LLWebSocketManager.Instance.Send(websocketKey, json);
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

            //string hotwords = "{\'你好\':20,\'查询\':30}";
            //string firstbuff = $"{{\"mode\": \"{asrmode}\", \"chunk_size\": [{chunk_size[0]},{chunk_size[1]},{chunk_size[2]}], \"chunk_interval\": {chunk_interval},\"hotwords\": \"{hotwords}\", \"wav_name\": \"microphone\", \"is_speaking\": true, \"itn\":false}}";
            //LLWebSocket.Instance.Send(firstbuff);
            //ClientSendAudioFunc(firstbuff);



            var hotwords = new Dictionary<string, int>
            {
                {"小园", 30},
                {"你好", 30},
                {"查询", 20},
                {"切换", 20},
                {"唤醒", 20},
                {"关闭", 20},
                {"退出", 20},
                {"帮助", 20},
                {"设置", 20},
                {"语音", 20},
                {"音量", 20},
                {"声音", 20},
                {"监控", 20},
                {"切换态势", 20},
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


    public class FunASR_MessageHandler : LLRecognizeHandlerBase
    {
        
        public override void ReceiveMessages(string message)
        {
            base.ReceiveMessages(message);
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
            //Debug.Log("message: " + data.text);
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
            RecogniseString(rec_text, isAwake => { 
                if (isAwake)
                {
                    //唤醒以后清空之前识别的内容
                    ClearAllText();
                    Debug.Log("isAwake");
                }else {
                    //失去唤醒以后清空之前识别的内容，重新开始等待唤醒
                    ClearAllText();
                    Debug.Log("noAwake");
                }
            });
        }

        public override void OnIdleLongTime()
        {
            base.OnIdleLongTime();
            ClearAllText();
        }

        ///<summary>
        ///清空所有识别内容
        ///</summary>
        public void ClearAllText()
         {
             offline_text = "";
             rec_text = "";
             online_text = "";
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

        ///<summary>
        ///发送is_speaking消息
        ///</summary>
        ///<param name="is_speaking"></param>
        ///<returns></returns>
        public void SendIsSpeaking(bool is_speaking)
        {
            OnIsSpeakingCallback?.Invoke(is_speaking);
        }
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

    /// <summary>
    /// 是否在说话
    /// </summary>
    [System.Serializable]
    public class LLFunSpeakingState { 
           public bool is_speaking = true;
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
