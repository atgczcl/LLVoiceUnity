using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System;
using NPinyin;
using System.Text;
using UnityEngine.Events;
using LLVoice.Tools;


namespace LLVoice.Voice
{
    /// <summary>
    /// 语音唤醒识别
    /// </summary>
    public class LLSpeechRecognition
    {
        public static string AI_Name = "小智";
        public static string AI_Name_Pinyin = "xiao zhi";
        /// <summary>
        /// 简码
        /// </summary>
        public static string AI_Name_Short = "XZ";

        /// <summary>
        /// 检查文本是否包含三个属性中的任何一个
        /// </summary>
        /// <param name="text">输入文本</param>
        /// <returns>如果包含则返回true，否则返回false</returns>
        public static bool ContainsKeyword(string text)
        {
            // 优先匹配汉字
            if (text.Contains(AI_Name))
            {
                return true;
            }

            // 转换为拼音并匹配拼音
            string pinyin = Pinyin.GetPinyin(text);
            if (pinyin.Contains(AI_Name_Pinyin, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // 转换为简码并匹配简码
            string shortCode = Pinyin.GetInitials(text);
            if (shortCode.Contains(AI_Name_Short, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }

    public class LLRecognizeHandlerBase: CommonMonoBehaviour
    {
        //public Queue<LLFunMessage> messageQueue = new Queue<LLFunMessage>();
        public UnityEvent<string> OnRecogniseCallback;
        public string recognition_text = "";
        /// <summary>
        /// 是否唤醒对话
        /// </summary>
        public bool isAwake = false;
        /// <summary>
        /// 是否正在说话
        /// </summary>
        public bool isSpeaking = false;
        /// <summary>
        /// 结束
        /// </summary>
        public string RecognitionEndTimeName = "LLRecognizeHandlerBase";
        public string OnIdleLongTimeName = "OnIdleLongTimeName";
        /// <summary>
        /// 是否唤醒对话 
        /// </summary>
        public Action<bool> isAwakeCallback;
        /// <summary>
        /// 是否结束对话回调
        /// </summary>
        public Action<bool> OnIsSpeakingCallback;
        /// <summary>
        /// 闲置超过时间10秒后的回调
        /// </summary>
        public Action OnIdleLongTimeCallback;

        public override void Awake()
        {
            base.Awake();
            SetIdleLongTimeState(true);
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="json_message">json 字符串</param>
        public virtual void ReceiveMessages(string json_message)
        {
            
        }

        public virtual void RecogniseString(string text, Action<bool> isAwakeCallback) {
            if (string.IsNullOrEmpty(text))return;

            this.isAwakeCallback = isAwakeCallback;
            //没有唤醒的情况下，检测是否唤醒了LLSpeechRecognition
            if (isAwake) {
                //接收消息
                recognition_text = text;
                //这个地方会把TimeName的值覆盖掉，空等待消失，现在是连续说话的情况，2秒内没有新的消息，则发送
                //并且消除了等待和唤醒状态，重新开始
                //监听两秒内是否有新的消息，如果有则不发送，如果没有则发送
                //两秒内没有新的消息，则发送
                SGTimer.StartTimer(RecognitionEndTimeName, 3, 1, () => {
                    ShowMessages(recognition_text);
                });
            }
            else
            {
                if (LLSpeechRecognition.ContainsKeyword(text))
                {
                    //唤醒了，记录唤醒状态，5秒后自动关闭
                    isAwake = true;
                    isSpeaking = true;
                    recognition_text = "";
                    SetIdleLongTimeState(false);
                    //这种情况会发生在唤醒后，又接收不到其他消息，此时需要关闭唤醒状态，默认等待5秒
                    SGTimer.StartTimer(RecognitionEndTimeName, 5, 1, () => {
                        //7秒后关闭了，再次打开需要重新唤醒
                        ShowMessages(recognition_text);
                    });
                    isAwakeCallback?.Invoke(true);
                }
                else
                {
                    Debug.Log("没有唤醒:" + text);
                }
            }
        }

        ///<summary>
        /// 显示消息
        ///</summary>
        ///<param name="message"></param>
        public virtual void ShowMessages(string message) {
            Debug.Log($"识别结束】】】】:{message}");
            isAwake = false;
            isSpeaking = false;
            //到时间发送收到的消息
            if (!string.IsNullOrEmpty(message)) { 
                OnRecogniseCallback?.Invoke(message);
            }
            recognition_text = "";
            isAwakeCallback?.Invoke(false);
            //发送说话结束的回调
            OnIsSpeakingCallback?.Invoke(false);
            //等待3秒后isSpeakingCallback为true, 启动麦克风发送数据
            SGTimer.StartTimer(RecognitionEndTimeName, 3, 1, () => {
                OnIsSpeakingCallback?.Invoke(true);
            });
            SetIdleLongTimeState(true);
            Debug.Log(message);
        }

        /// <summary>
        /// 发送超长时间闲置状态回调
        /// </summary>
        public void SetIdleLongTimeState(bool isIdleLongTime) {
            if (isIdleLongTime) {
                SGTimer.StartTimer(OnIdleLongTimeName, 5, -1, () => {
                    OnIdleLongTime();
                });
            }else {
                SGTimer.Stop(OnIdleLongTimeName);
            }
        }

        /// <summary>
        /// 发送超长时间闲置状态回调
        /// </summary>
        public virtual void OnIdleLongTime() {
            if (!isAwake)
                OnIdleLongTimeCallback?.Invoke();
            Debug.LogError($"闲置时间长了，清理数据！");
        }

    }
}