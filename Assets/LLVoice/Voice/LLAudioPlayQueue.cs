using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 音频播放队列
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
        //是否在播放
        PlayAudio();
    }

    //依次播放队列里面的音频
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
            //定时检测是否播放完毕，播放完毕则继续播放下一个
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
