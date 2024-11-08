using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LLVoice.Voice.LLAudioPlayQueue;


namespace LLVoice.Voice
{
    /// <summary>
    /// 音频播放队列
    /// </summary>
    public class LLAudioPlayQueue : MonoBehaviour
    {
        /// <summary>
        /// 播放队列类型
        /// </summary>
        public enum AudioPlayQueueType {
            /// <summary>
            /// 预加载的音频
            /// </summary>
            PreLoad,
            /// <summary>
            /// 聊天中的音频队列
            /// </summary>
            Queue,
        }

        //audioclip queue
        public Queue<AudioClip> audioQueue = new Queue<AudioClip>();
        public AudioSource audioSource;
        public LLPreAudioData preAudioData;
        /// <summary>
        /// 是否是队列或者预加载的播放
        /// </summary>
        public AudioPlayQueueType audioPlayQueueType = AudioPlayQueueType.PreLoad;


        public void Enqueue(AudioClip audioClip)
        {
            if (audioClip == null)
            {
                Debug.LogError("audioClip is null");
                return;
            }
            audioQueue.Enqueue(audioClip);
            //PlayQueueAudio();
            Debug.LogError($"收到音频 {audioPlayQueueType}|{audioSource.isPlaying}|{(audioPlayQueueType == AudioPlayQueueType.Queue && audioSource.isPlaying)}");
        }

        public AudioClip Dequeue()
        {
            return audioQueue.Dequeue();
        }

        private void Update()
        {
            //是否在播放
            TryPlayQueueAudio();
        }

        /// <summary>
        /// 依次播放队列里面的音频
        /// 如果是队列模式，需要判断是否正在播放
        /// </summary>
        public void TryPlayQueueAudio()
        {
            //PreLoad模式可以直接播放
            if (audioPlayQueueType == AudioPlayQueueType.PreLoad)
            {
                if (audioQueue.Count > 0)
                {
                    PlayAudio(Dequeue());
                    audioPlayQueueType = AudioPlayQueueType.Queue;  
                }
            } 
            else 
            {
                //Queue模式播放
                if (audioSource.isPlaying)
                {
                    return;
                }

                if (audioQueue.Count > 0)
                {
                    PlayAudio(Dequeue());
                    audioPlayQueueType = AudioPlayQueueType.Queue;
                }
                else
                {
                    audioPlayQueueType = AudioPlayQueueType.PreLoad;
                }
            }
        }

        /// <summary>
        /// 播放queue里面的音频
        /// </summary>
        public void PlayQueueAudio()
        {
            if (audioQueue.Count > 0)
            {
                PlayAudio(Dequeue());
                audioPlayQueueType = AudioPlayQueueType.Queue;  
            }
        }

        /// <summary>
        /// 播放指定clip
        /// </summary>
        public void PlayAudio(AudioClip audioClip)
        {
            if (audioClip == null)
            {
                Debug.LogError("audioClip is null");
                return;
            }
            audioSource.Stop();
            audioSource.clip = audioClip;
            audioSource.enabled = true;
            audioSource.Play();
        }

        /// <summary>
        /// 播放预设交互音频
        /// </summary>
        public void PlayPreWakeUpAudio() {
            audioPlayQueueType = AudioPlayQueueType.PreLoad;
            PlayAudio(preAudioData.GetRandomWakeUpAudio());
        }

        /// <summary>
        /// 播放预设再见音频
        /// </summary>
        public void PlayPreByeAudio() {
            audioPlayQueueType = AudioPlayQueueType.PreLoad;
            PlayAudio(preAudioData.再见);
        }

        /// <summary>
        /// 播放预设欢迎词
        /// </summary>
        public void PlayPreWelcomeAudio() {
            audioPlayQueueType = AudioPlayQueueType.PreLoad;
            PlayAudio(preAudioData.欢迎词);
        }

        /// <summary>
        /// 检测音频播放完毕
        /// </summary>
        /// <returns></returns>
        IEnumerator CheckAudioPlayEnd()
        {
            while (audioSource.isPlaying)
            {
                yield return null;
            }
            PlayQueueAudio();
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }

    /// <summary>
    /// 预置音频类
    /// </summary>
    [System.Serializable]
    public class LLPreAudioData { 
        public AudioClip 欢迎词;
        public AudioClip 再见;
        public AudioClip 请说;
        public AudioClip 请问;
        public AudioClip 我在;
        public AudioClip 有什么帮助;


        /// <summary>
        /// 随机获取，请说，我在，有什么帮助，请问四个交互语音
        /// 默认我在，我在频率提高
        /// </summary>
        public AudioClip GetRandomWakeUpAudio()
        {
            //0-5我在提高频率
            int index = Random.Range(0, 5);
            switch (index)
            {
                case 0:
                    return 请说;
                case 1:
                    return 我在;
                case 2:
                    return 有什么帮助;
                case 3:
                    return 请问;
                default:
                    return 我在;
            }
        }
    }
}