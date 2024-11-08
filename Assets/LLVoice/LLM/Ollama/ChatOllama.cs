using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace LLVoice.LLM
{
    public class ChatOllama : LLM
    {
        /// <summary>
        /// AI�趨
        /// </summary>
        public string m_SystemSetting = string.Empty;
        /// <summary>
        /// ����ģ��,ģ�������������
        /// </summary>
        public ModelType m_GptModel = ModelType.qwen7b;

        /// <summary>
        /// ģ��ö�ٺ��ַ�����Ӧ
        /// </summary>
        public List<OllamaNameModel> m_ModelList = new List<OllamaNameModel>() {
            new OllamaNameModel("llama3", ModelType.llama3),
            new OllamaNameModel("qianwen:7b", ModelType.qwen7b)
    };

        private void Start()
        {
            //����ʱ�����AI�趨
            m_DataList.Add(new SendData("system", m_SystemSetting));
        }

        /// <summary>
        /// ������Ϣ
        /// </summary>
        /// <returns></returns>
        public override void PostMsg(string _msg, Action<string> _callback)
        {
            base.PostMsg(_msg, _callback);
        }

        /// <summary>
        /// ���ýӿ�
        /// </summary>
        /// <param name="_postWord"></param>
        /// <param name="_callback"></param>
        /// <returns></returns>
        public override IEnumerator Request(string _postWord, System.Action<string> _callback)
        {
            stopwatch.Restart();
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                string modelName = m_ModelList.Find(x => x.model == m_GptModel).name;
                if (string.IsNullOrEmpty(modelName)) modelName = m_GptModel.ToString();
                PostData _postData = new PostData
                {
                    model = modelName,
                    messages = m_DataList
                };

                string _jsonText = JsonUtility.ToJson(_postData);
                byte[] data = System.Text.Encoding.UTF8.GetBytes(_jsonText);
                request.uploadHandler = (UploadHandler)new UploadHandlerRaw(data);
                request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

                request.SetRequestHeader("Content-Type", "application/json");
                //request.SetRequestHeader("Authorization", string.Format("Bearer {0}", api_key));

                yield return request.SendWebRequest();

                if (request.responseCode == 200)
                {
                    string _msgBack = request.downloadHandler.text;
                    MessageBack _textback = JsonUtility.FromJson<MessageBack>(_msgBack);
                    if (_textback != null && _textback.message != null)
                    {

                        string _backMsg = _textback.message.content;
                        //��Ӽ�¼
                        m_DataList.Add(new SendData("assistant", _backMsg));
                        _callback(_backMsg);
                    }
                }
                else
                {
                    string _msgBack = request.downloadHandler.text;
                    Debug.LogError(_msgBack);
                }

                stopwatch.Stop();
                Debug.Log("Ollama��ʱ��" + stopwatch.Elapsed.TotalSeconds);
            }
        }

        #region ���ݶ���

        public enum ModelType
        {
            qwen7b,
            llama3,
        }

        [Serializable]
        public class PostData
        {
            public string model;
            public List<SendData> messages;
            public bool stream = false;//��ʽ
        }
        [Serializable]
        public class MessageBack
        {
            public string created_at;
            public string model;
            public Message message;
        }

        [Serializable]
        public class Message
        {
            public string role;
            public string content;
        }

        [Serializable]
        public class OllamaNameModel
        {
            public ModelType model;
            public string name;

            public OllamaNameModel(string name, ModelType model)
            {
                this.name = name;
                this.model = model;
            }
        }

        #endregion

    }
}