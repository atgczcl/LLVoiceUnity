using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LLVoice.Voice.LLAudioPlayQueue;


namespace LLVoice.Voice
{
    /// <summary>
    /// ��Ƶ���Ŷ���
    /// </summary>
    public class LLAudioPlayQueue : MonoBehaviour
    {
        /// <summary>
        /// ���Ŷ�������
        /// </summary>
        public enum AudioPlayQueueType {
            /// <summary>
            /// Ԥ���ص���Ƶ
            /// </summary>
            PreLoad,
            /// <summary>
            /// �����е���Ƶ����
            /// </summary>
            Queue,
        }

        //audioclip queue
        public Queue<AudioClip> audioQueue = new Queue<AudioClip>();
        public AudioSource audioSource;
        public LLPreAudioData preAudioData;
        /// <summary>
        /// �Ƿ��Ƕ��л���Ԥ���صĲ���
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
            PlayQueueAudio();
        }

        public AudioClip Dequeue()
        {
            return audioQueue.Dequeue();
        }

        private void Update()
        {
            //�Ƿ��ڲ���
            PlayQueueAudio();
        }

        //���β��Ŷ����������Ƶ
        public void PlayQueueAudio()
        {
            //�����ǰ���ڲ��ţ�������Ԥ���ص���Ƶ���򲻲���
            if (audioPlayQueueType == AudioPlayQueueType.Queue && audioSource.isPlaying)
            {
                return;
            }

            if (audioQueue.Count > 0)
            {
                PlayAudio(Dequeue());
            }
            audioPlayQueueType = AudioPlayQueueType.Queue;
        }

        /// <summary>
        /// ����ָ��clip
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
        /// ����Ԥ�轻����Ƶ
        /// </summary>
        public void PlayPreWakeUpAudio() {
            audioPlayQueueType = AudioPlayQueueType.PreLoad;
            PlayAudio(preAudioData.GetRandomWakeUpAudio());
        }

        /// <summary>
        /// ����Ԥ���ټ���Ƶ
        /// </summary>
        public void PlayPreByeAudio() {
            audioPlayQueueType = AudioPlayQueueType.PreLoad;
            PlayAudio(preAudioData.�ټ�);
        }

        /// <summary>
        /// ����Ԥ�軶ӭ��
        /// </summary>
        public void PlayPreWelcomeAudio() {
            audioPlayQueueType = AudioPlayQueueType.PreLoad;
            PlayAudio(preAudioData.��ӭ��);
        }

        /// <summary>
        /// �����Ƶ�������
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
    /// Ԥ����Ƶ��
    /// </summary>
    [System.Serializable]
    public class LLPreAudioData { 
        public AudioClip ��ӭ��;
        public AudioClip �ټ�;
        public AudioClip ��˵;
        public AudioClip ����;
        public AudioClip ����;
        public AudioClip ��ʲô����;


        /// <summary>
        /// �����ȡ����˵�����ڣ���ʲô�����������ĸ���������
        /// Ĭ�����ڣ�����Ƶ�����
        /// </summary>
        public AudioClip GetRandomWakeUpAudio()
        {
            //0-5�������Ƶ��
            int index = Random.Range(0, 5);
            switch (index)
            {
                case 0:
                    return ��˵;
                case 1:
                    return ����;
                case 2:
                    return ��ʲô����;
                case 3:
                    return ����;
                default:
                    return ����;
            }
        }
    }
}