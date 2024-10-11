using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// ��Ƶ���Ŷ���
/// </summary>
public class LLAudioPlayQueue: MonoBehaviour
{
    //audioclip queue
    public Queue<AudioClip> audioQueue = new Queue<AudioClip>();
    public AudioSource audioSource;

    public void Enqueue(AudioClip audioClip)
    {
        if (audioClip == null)
        {
            Debug.LogError("audioClip is null");
            return;
        }
        audioQueue.Enqueue(audioClip);
        PlayAudio();
    }

    public AudioClip Dequeue()
    {
        return audioQueue.Dequeue();
    }

    private void Update()
    {
        //�Ƿ��ڲ���
        PlayAudio();
    }

    //���β��Ŷ����������Ƶ
    public void PlayAudio()
    {
        if (audioSource.isPlaying)
        {
            return;
        }

        if (audioQueue.Count > 0)
        {
            audioSource.clip = Dequeue();
            audioSource.Play();
            //��ʱ����Ƿ񲥷���ϣ�������������������һ��
            //StartCoroutine(CheckAudioPlayEnd());
        }
    }
    //IEnumerator CheckAudioPlayEnd()
    //{
    //    while (audioSource.isPlaying)
    //    {
    //        yield return null;
    //    }
    //    PlayAudio();
    //}

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
