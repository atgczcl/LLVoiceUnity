using LLVoice.Voice;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LLVoice
{

    public class LLChatTest : MonoBehaviour
    {
        public TMPro.TMP_InputField inputField;


        public void SendGenAudioWavMsg()
        {
            LLFunASR.Instance.SendTTS(inputField.text);
        }

        //����Ai�����ı�
        public void SendAiMsg()
        {
            LLFunASR.Instance.OnResultCallbacks(inputField.text);
        }
    }
}