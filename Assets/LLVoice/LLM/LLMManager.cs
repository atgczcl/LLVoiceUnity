using LLVoice.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LLVoice.LLM
{
    public class LLMManager : MonoSingleton<LLMManager>
    {
        public LLMOpenAI llmOpenAI;

        internal void StopChat()
        {
            if (llmOpenAI != null)LLMOpenAI.StopChat();
        }
    }
}