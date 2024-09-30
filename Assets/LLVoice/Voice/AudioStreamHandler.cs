using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Numerics;
using System.Collections.Generic;
using Unity.Collections;

//public class AudioStreamHandler : MonoBehaviour
using System.Linq;
using System;

//public class AudioStreamHandler : MonoBehaviour
//{
//    private UnityWebRequest _webRequest;
//    private AudioStreamProcessor _audioProcessor;
//    private AudioSource _audioSource;

//    void Start()
//    {
//        string inputText = "hello world";
//        StartCoroutine(StartStream(inputText));
//    }

//    IEnumerator StartStream(string inputText)
//    {
//        string url = "http://127.0.0.1:8080/inference/stream";

//        _webRequest = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
//        _webRequest.SetRequestHeader("Content-Type", "application/json");

//        var requestBody = new
//        {
//            query = inputText,
//            prompt_text = "确保已部署CosyVoice项目，已将 CosyVoice-api中的api.py放入，并成功启动了 api.py。",
//            prompt_speech = "yy.wav"
//        };

//        _webRequest.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(requestBody)));
//        _webRequest.downloadHandler = new DownloadHandlerBuffer();

//        _audioSource = gameObject.AddComponent<AudioSource>();
//        _audioSource.playOnAwake = false;

//        _audioProcessor = new AudioStreamProcessor();

//        yield return _webRequest.SendWebRequest();

//        if (_webRequest.isNetworkError || _webRequest.isHttpError)
//        {
//            Debug.LogError(_webRequest.error);
//        }
//        else
//        {
//            _audioProcessor.ProcessStream(_webRequest.downloadHandler);
//        }
//    }

//    private class AudioStreamProcessor
//    {
//        private AudioStreamBuffer _buffer;
//        private bool _isPlaying;

//        public AudioStreamProcessor()
//        {
//            _buffer = new AudioStreamBuffer();
//            _isPlaying = false;
//        }

//        public void ProcessStream(DownloadHandlerBuffer handler)
//        {
//            using (var reader = new System.IO.MemoryStream(handler.data))
//            {
//                while (true)
//                {
//                    var result = reader.Read();
//                    if (result.done)
//                        break;

//                    var combinedValue = new byte[_buffer.Leftover.Length + result.value.Length];
//                    Buffer.BlockCopy(_buffer.Leftover, 0, combinedValue, 0, _buffer.Leftover.Length);
//                    Buffer.BlockCopy(result.value, 0, combinedValue, _buffer.Leftover.Length, result.value.Length);

//                    var byteLength = combinedValue.Length;
//                    var remainder = byteLength % 4;
//                    var validLength = byteLength - remainder;

//                    var validData = new byte[validLength];
//                    Buffer.BlockCopy(combinedValue, 0, validData, 0, validLength);

//                    _buffer.Leftover = new byte[remainder];
//                    Buffer.BlockCopy(combinedValue, validLength, _buffer.Leftover, 0, remainder);

//                    var float32Array = ConvertBytesToFloat32Array(validData);
//                    _buffer.AudioBufferQueue.Add(float32Array);

//                    ProcessBuffer();
//                }
//            }
//        }

//        private void ProcessBuffer()
//        {
//            if (_isPlaying || !_buffer.AudioBufferQueue.Any())
//                return;

//            var tmpBufQueue = _buffer.AudioBufferQueue.ToList();
//            _buffer.AudioBufferQueue.Clear();

//            var totalLength = tmpBufQueue.Sum(chunk => chunk.Length);
//            var combinedArray = new float[totalLength];

//            var offset = 0;
//            foreach (var chunk in tmpBufQueue)
//            {
//                Buffer.BlockCopy(chunk, 0, combinedArray, offset * sizeof(float), chunk.Length * sizeof(float));
//                offset += chunk.Length;
//            }

//            var audioClip = CreateAudioClip(combinedArray);
//            PlayAudioClip(audioClip);
//        }

//        private void PlayAudioClip(AudioClip clip)
//        {
//            if (_isPlaying)
//                return;

//            _isPlaying = true;
//            _audioSource.clip = clip;
//            _audioSource.Play();
//        }

//        private AudioClip CreateAudioClip(float[] samples)
//        {
//            var audioClip = AudioClip.Create("GeneratedAudio", samples.Length, 1, 22050, false);
//            audioClip.SetData(samples, 0);
//            return audioClip;
//        }

//        private Float32Array ConvertBytesToFloat32Array(byte[] bytes)
//        {
//            var floatArray = new float[bytes.Length / 4];
//            Buffer.BlockCopy(bytes, 0, floatArray, 0, bytes.Length);
//            return floatArray;
//        }
//    }

//    private class AudioStreamBuffer
//    {
//        public List<float[]> AudioBufferQueue { get; } = new List<float[]>();
//        public byte[] Leftover { get; set; } = new byte[0];
//    }

    

//    private class StreamReader
//    {
//        private readonly System.IO.MemoryStream _stream;

//        public StreamReader(System.IO.MemoryStream stream)
//        {
//            _stream = stream;
//        }

//        public ReadResult Read()
//        {
//            var buffer = new byte[4096];
//            var bytesRead = _stream.Read(buffer, 0, buffer.Length);
//            return new ReadResult { value = new ArraySegment<byte>(buffer, 0, bytesRead), done = _stream.Length == _stream.Position };
//        }
//    }

//    private class ReadResult
//    {
//        public ArraySegment<byte> value;
//        public bool done;
//    }
//}
