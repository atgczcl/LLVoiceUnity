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
        /// ��ʾ��ʽģ��latency���ã�`[5, 10, 5]`����ʾ��ǰ��ƵΪ600ms�����һؿ�300ms���ֿ�300ms��
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
            Debug.Log("websocket �յ���Ϣ: " + msg);
            msgHandler.ReceiveMessages(msg);
            //OnMessageCallback?.Invoke(msg);
        }

        public void Init()
        {
            //��ʼ��
            ClientFirstConnOnline();
        }

        private void Update()
        {
            if (msgHandler != null) msgHandler.DispatchMessages();
        }

        /// <summary>
        /// �ͻ����״�������Ϣ
        /// </summary>
        /// <param name="asrmode">online, offline, 2pass</param>
        /// <returns></returns>
        public bool ClientFirstConnOnline(string asrmode = "2pass")
        {
            // ����˵����
            // `mode`��`offline`����ʾ����ģʽΪһ�仰ʶ��`online`����ʾ����ģʽΪʵʱ����ʶ��`2pass`����ʾΪʵʱ����ʶ�𣬲���˵����β��������ģ�ͽ��о���
            //`wav_name`����ʾ��Ҫ������Ƶ�ļ���
            //`wav_format`����ʾ����Ƶ�ļ���׺����ֻ֧��pcm��Ƶ��
            //`is_speaking`����ʾ�Ͼ�β�㣬���磬vad�и�㣬����һ��wav����
            //`chunk_size`����ʾ��ʽģ��latency���ã�`[5, 10, 5]`����ʾ��ǰ��ƵΪ600ms�����һؿ�300ms���ֿ�300ms��
            //`audio_fs`����������ƵΪpcm�����ǣ���Ҫ������Ƶ�����ʲ���
            //`hotwords`�����ʹ���ȴʣ���Ҫ�����˷����ȴ����ݣ��ַ���������ʽΪ "{"����Ͱ�":20,"ͨ��ʵ����":30}"
            //`itn`: �����Ƿ�ʹ��itn��Ĭ��True
            // jsonresult={"chunk_interval":10,"chunk_size":[5,10,5],"hotwords":"{\"\\u4f60\\u597d\": 20, \"\\u67e5\\u8be2\": 30}","is_speaking":true,"itn":true,"mode":"2pass","wav_name":"microphone"}, msg_data->msg={"access_num":0,"audio_fs":16000,"is_eof":false,"itn":true,"mode":"2pass","wav_format":"pcm","wav_name":"microphone"}
            string hotwords = "{{\'���\':20,\'��ѯ\':30}}";
            //string hotwords = "����Ͱ� 20\n��ĦԺ 20\nҹ��Ʈ�� 20\n";
            string firstbuff = $"{{\"mode\": \"{asrmode}\", \"chunk_size\": [{chunk_size[0]},{chunk_size[1]},{chunk_size[2]}], \"chunk_interval\": {chunk_interval},\"hotwords\": \"{hotwords}\", \"wav_name\": \"microphone\", \"is_speaking\": true}}";
                       //, asrmode, chunk_size[0], chunk_size[1], chunk_size[2], chunk_interval, hotwords);
            LLWebSocket.Instance.Send(firstbuff);
            //ClientSendAudioFunc(firstbuff);
            return true;
        }

        public bool ClientSendAudioFunc(byte[] buff)    //ʵʱʶ��
        {
            int wave_buffer_collectfrequency = 16000;
            ////������Ƶ����
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
        /// �ַ���Ϣ
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


        private string recbuff = string.Empty;//�����ۼƻ�������
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

            if (meg.is_final)//δ������ǰʶ��
            {
                recbuff = string.Empty;
            }
        }

        private void UpdateConsoleText(string text)
        {
            // ���¿���̨�ı�
            Debug.Log($"ʶ����: {text}");
            OnMessageCallback?.Invoke(text);
        }

        // ������Щ�����Ѿ������
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
