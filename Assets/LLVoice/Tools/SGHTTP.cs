using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using LLVoice.Tools;

namespace cscec.IOC
{
    /// <summary>
    /// Http请求
    /// </summary>
    public class SGHTTP : MonoSingleton<SGHTTP>
    {
        

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