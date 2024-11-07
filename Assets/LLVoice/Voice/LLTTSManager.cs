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
    /// �ϳ�����
    /// </summary>
    /// <param name="url">http://127.0.0.1:8080/v1/tts</param>
    /// <param name="text"></param>
    /// <param name="speaker">����Ů,������,Ӣ��Ů,Ӣ����,������,����Ů,����Ů</param>
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
    /// �ϳ�����
    /// </summary>
    /// <param name="content"></param>
    /// <param name="speeker">['����Ů', '������', '������', '����Ů', 'Ӣ��Ů', 'Ӣ����', '����Ů']</param>
    /// <param name="onResult"></param>
    public async void GetTTSStream_SFT(string content, Action<AudioClip> onResult, string speeker = null)
    {
        Dictionary<string, string> formData = new();
        //  {
        //      query: data,
        //  prompt_text: 'ȷ���Ѳ���CosyVoice��Ŀ���ѽ� CosyVoice-api�е�api.py���룬���ɹ������� api.py��',
        //  prompt_speech: "yy.wav"
        //}
        //            //string cc = @"����������9��29�յ� (���� �߿�)�ݵ���רҵ�����ݣ�����9��29��14ʱ38�֣�2024����쵵(10��1�ա�10��7��)������Ԥ����Ʊ��ͻ��1��Ԫ����749�֡���ԡ��֮·����־Ը��������֮ս�����е���Ԥ��Ʊ����ǰ��λ��
        //���ݵ���רҵ����ӳ������ʾ��������쵵����10����Ƭ��ӳ�����Ǿ��顢ս����������������ѡ���ͥ���ƻá�ð�ա��ഺ����ʷ�����衢������ϲ�硢��õȶ������ͣ���������ӰƬ8���������������ϱ�ȥ����쵵��2�������Ҷ�������������ģ��ӰƬ��Ҳ�ǽ�����ͷ����������ӰƬ��������һ�ι��쵵��";
        string cc = @"����������9��29�յ� (���� �߿�)�ݵ���רҵ�����ݣ�����9��29��14ʱ38�֣�2024����쵵(10��1�ա�10��7��)������Ԥ����Ʊ��ͻ��1��Ԫ����749�֡���ԡ��֮·����־Ը��������֮ս�����е���Ԥ��Ʊ����ǰ��λ��";
        formData?.TryAdd("query", content);
        formData?.TryAdd("speaker", speeker == null ? "����Ů" : speeker);
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
    /// ������û�����ƣ���Ҫ�޸Ĵ���
    /// </summary>
    /// <param name="content"></param>
    /// <param name="onResult"></param>
    public async void GetTTSStream_Clone(string content, Action<AudioClip> onResult)
    {
        Dictionary<string, string> formData = new();
        //  {
        //      query: data,
        //  prompt_text: 'ȷ���Ѳ���CosyVoice��Ŀ���ѽ� CosyVoice-api�е�api.py���룬���ɹ������� api.py��',
        //  prompt_speech: "yy.wav"
        //}
        //            //string cc = @"����������9��29�յ� (���� �߿�)�ݵ���רҵ�����ݣ�����9��29��14ʱ38�֣�2024����쵵(10��1�ա�10��7��)������Ԥ����Ʊ��ͻ��1��Ԫ����749�֡���ԡ��֮·����־Ը��������֮ս�����е���Ԥ��Ʊ����ǰ��λ��
        //���ݵ���רҵ����ӳ������ʾ��������쵵����10����Ƭ��ӳ�����Ǿ��顢ս����������������ѡ���ͥ���ƻá�ð�ա��ഺ����ʷ�����衢������ϲ�硢��õȶ������ͣ���������ӰƬ8���������������ϱ�ȥ����쵵��2�������Ҷ�������������ģ��ӰƬ��Ҳ�ǽ�����ͷ����������ӰƬ��������һ�ι��쵵��";
        string cc = @"����������9��29�յ� (���� �߿�)�ݵ���רҵ�����ݣ�����9��29��14ʱ38�֣�2024����쵵(10��1�ա�10��7��)������Ԥ����Ʊ��ͻ��1��Ԫ����749�֡���ԡ��֮·����־Ը��������֮ս�����е���Ԥ��Ʊ����ǰ��λ��";
        formData?.TryAdd("query", content);
        formData?.TryAdd("prompt_text", "ȷ���Ѳ���CosyVoice��Ŀ���ѽ� CosyVoice-api�е�api.py���룬���ɹ������� api.py��");
        formData?.TryAdd("prompt_speech", "yy.wav");
        string jsonData = JsonConvert.SerializeObject(formData);
        string url = $"{TTSUrl}inference/streamclone";
        await PostTTSStream(url, jsonData, pcmData => {
            AudioClip audioClip = ConvertPCM16ToAudioClip(pcmData, 22050);
            onResult?.Invoke(audioClip);
        });
    }

    /// <summary>
    /// ��ȡTTS ��json����ʹ��base64����
    /// </summary>
    /// <param name="content"></param>
    /// <param name="onResult"></param>
    /// <param name="speeker"></param>
    public async void GetTTSStream_SFT_Json(string content, Action<AudioClip> onResult, string speeker = null)
    {
        Dictionary<string, string> formData = new();
        formData?.TryAdd("query", content);
        formData?.TryAdd("speaker", speeker == null ? "����Ů" : speeker);
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
        //����ʱ��������xxx.wav
        LLAudioConverter.SavePcmToWavFile(totalPCMData.ToArray(), fileName + ".wav");
    }

    /// <summary>
    /// ��PCM16ת��ΪAudioClip
    /// </summary>
    /// <param name="pcmData"></param>
    /// <param name="sampleRate"></param>
    /// <returns></returns>
    public AudioClip ConvertPCM16ToAudioClip(byte[] pcmData, int sampleRate)
    {
        int sampleCount = pcmData.Length / 2; // ÿ������ռ��2�ֽ�
        float[] floatArray = new float[sampleCount];

        int floatSize = sizeof(float);
        List<float> floats = new List<float>();

        for (int i = 0; i < pcmData.Length; i += floatSize)
        {
            // ����Ƿ����㹻���ֽ����γ�һ��������
            if (i + floatSize > pcmData.Length) break;

            // �����ֽڵ�������
            float value = BitConverter.ToSingle(pcmData, i);
            floats.Add(value);
        }

        // ����AudioClip
        AudioClip audioClip = AudioClip.Create("PCMClip", floats.Count, 1, sampleRate, false);
        audioClip.SetData(floats.ToArray(), 0);

        return audioClip;
    }



    /// <summary>
    /// Ollama �ʴ�ӿ�
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
            request.Headers.Add("Accept", "application/octet-stream"); // ��������ֽ���
            request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            //��ʱ5����
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
                        Debug.LogError($"�յ����� length: {validBytes.Length}|{totalBytes}|{responseStream.CanRead}");
                        //onResult?.Invoke(chunk); // ����ÿ���ֽ�����
                        onResult?.Invoke(validBytes); // ����ÿ���ֽ�����
                    }
                    //Debug.LogError($"��ȡ���� length: {leftOver.Length}|{responseStream.CanRead}|{responseStream.CanSeek}");
                    //Debug.LogError($"�յ����� length: {totalBytes.Count}");

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
                ///ȷ������ɹ��������׳��쳣
                response.EnsureSuccessStatusCode();

                using (var responseStream = await response.Content.ReadAsStreamAsync())
                using (var reader = new System.IO.StreamReader(responseStream))
                {
                    string request_id = response.Headers.GetValues("X-Request-ID").FirstOrDefault();
                    //������request_id�б�
                    if (!string.IsNullOrEmpty(request_id))RequestIdList.AddRequestID(request_id);
                    string line;
                    while (!IsDestroy && (line = await reader.ReadLineAsync()) != null)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            //{ "model":"qwen:7b","created_at":"2024-03-27T07:11:33.0059464Z","message":{ "role":"assistant","content":"\n"},"done":false}
                            LLTTSSteamDataBlock responesModelData = JsonConvert.DeserializeObject<LLTTSSteamDataBlock>(line);
                            var data = responesModelData.ToBytes();
                            Debug.LogError($"�յ�����:{data.Length}|{request_id}");
                            onResult?.Invoke(data);
                        }
                    }
                    //�Ƴ�request_id�б� ������Զ����������÷�����Ϣ
                    RequestIdList.RemoveRequestID(request_id);
                }
            }
        }
    }




    /// <summary>
    /// ��ʣ�����ݺ͵�ǰ���ݺϲ���������ʣ������
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
    /// ����ֹͣ��������
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
            // ��������ͷ
            www.SetRequestHeader("Content-Type", "application/json");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(RequestIdList.ToJson());
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            // ��������
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

    //base64����dataתΪbyte[]
    public byte[] ToBytes()
    {
        return System.Convert.FromBase64String(data);
    }
}

/// <summary>
/// TTS����������Ƶ���ݿ�
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
    /// json��ʽ��
    /// </summary>
    /// <returns></returns>
    public string ToJson()
    {
        return JsonConvert.SerializeObject(this);
    }
}
