using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Unity.Collections;
using UnityEngine;

/// <summary>
/// 定时器管理类
/// </summary>
public class SGTimer
{
    private SGTimeMono _inst;//内静态实例
    public static TimerPool<SGTimeBean> timerPool;//定时器对象池
    public static SGTimer _playloadTimer;

    /// <summary>
    /// 字典缓存transform对象
    /// </summary>
    public static readonly ConcurrentDictionary<Component, SGTimeBean> TransformDic = new();

    #region 静态公共定时器
    /// <summary>
    /// 静态定时器,做公共定时器；
    /// </summary>
    /// <param name="timerName">定时器名称，同名替换，Null/空自动生成名称不重复</param>
    /// <param name="delayTime">延时时间单位：秒，帧率uint,</param>
    /// <param name="repeatRate">循环次数，Uint整数次数，-1无限循环，0=update无限模式</param>
    /// <param name="callback">回调无参</param>
    /// <param name="isNeedStopCallback">定时未结束，主动Stop是否回调</param>
    /// <param name="groupTag">定时器按照标签分类</param>
    /// <returns></returns>
    public static SGTimeBean StartTimer(string timerName, float delayTime, int repeatRate, Action callback, bool isNeedStopCallback = false, string groupTag = "")
    {
        CheckPlayload();
        return _playloadTimer.StartTimer(timerName, delayTime, repeatRate, callback, isNeedStopCallback, groupTag);
    }

    /// <summary>
    /// 自管理定时器，内存自动回收
    /// </summary>
    public static SGTimeBean StartTimer(float delayTime, int repeatRate, Action callback, bool isNeedStopCallback = false, string groupTag = "")
    {
        CheckPlayload();
        return _playloadTimer.StartTimer(delayTime, repeatRate, callback, isNeedStopCallback, groupTag);
    }
    
    /// <summary>
    /// 停止定时器
    /// </summary>
    /// <param name="timerName"></param>
    /// <param name="isNeedStopCallback"></param>
    public static void Stop(string timerName, bool isNeedStopCallback = false)
    {
        CheckPlayload();
        _playloadTimer.StopTimer(timerName, isNeedStopCallback);
    }

    /// <summary>
    /// 启用一个mono Update 无限刷新
    /// </summary>
    /// <param name="callback">回调</param>
    /// <returns></returns>
    public static SGTimeBean StarUpdate(string timerName, Action callback)
    {
        CheckPlayload();
        return _playloadTimer.StartUpdate(timerName, callback);
    }

    /// <summary>
    /// 启动帧率刷新
    /// </summary>
    /// <param name="timerName">帧率刷新名称</param>
    /// <param name="framCount">帧率</param>
    /// <param name="callback">回调</param>
    /// <returns></returns>
    public static SGTimeBean StarFrameUpdate(string timerName, uint framCount, Action callback)
    {
        CheckPlayload();
        return _playloadTimer.StartFrameUpdate(timerName, framCount, callback);
    }

    /// <summary>
    /// 启动一个协程
    /// </summary>
    /// <param name="routine"></param>
    /// <returns></returns>
    public static Coroutine StartCoroutineMethod(IEnumerator routine)
    {
        CheckPlayload();
        return _playloadTimer.GetMono().StartCoroutine(routine);
    }

    /// <summary>
    /// 停止一个协程
    /// </summary>
    /// <param name="routine"></param>
    public static void StopCoroutineMethod(IEnumerator routine)
    {
        CheckPlayload();
        _playloadTimer.GetMono().StopCoroutine(routine);
    }

    private static void CheckPlayload()
    {
        if (_playloadTimer == null)
        {
            _playloadTimer = new SGTimer("PublicTimerRunner");
        }
    }
    #endregion

    #region 公共处理
    public SGTimer(string timerName = null)
    {
        if (timerPool == null)
        {
            timerPool = new TimerPool<SGTimeBean>(() => {
                return new SGTimeBean();
            }, (obj) =>
            {
                obj.Clear();
            }, 150);
        }
        if (_inst == null)
        {
            _inst = new GameObject("[" + (timerName == null ? "SGTimer" : timerName) + "]").AddComponent<SGTimeMono>();
            //_inst.hideFlags = HideFlags.HideInHierarchy;
            _inst.gameObject.SetActive(true);
            GameObject.DontDestroyOnLoad(_inst.gameObject);
        }
    }

    /// <summary>
    /// 时间分组标签删除
    /// </summary>
    /// <param name="groupTag"></param>
    /// <param name="isNeedStopCallback"></param>
    public void RemoveByGroupTag(string groupTag, bool isNeedStopCallback = false)
    {
        if (groupTag != null && groupTag != "")
        {
            _inst.RemoveByGroupTag(groupTag, isNeedStopCallback);
        }
    }

    /// <summary>
    /// 通用定时器
    /// </summary>
    /// <param name="timerName">定时器名称 如果传入定时器名称 则会替换同个名称的点时期对象，如果名称为null，则不会替换</param>
    /// <param name="delayTimeOrFrameCounter">延迟 正数是延迟时间 负数是几帧后执行 </param>
    /// <param name="repeatRate">-1无限循环</param>
    /// <param name="callback">回调</param>
    /// <param name="isNeedStopCallback">停止后还有回调</param>
    /// <param name="groupTag">分组标签</param>
    public SGTimeBean StartTimer(string timerName, float delayTimeOrFrameCounter, int repeatRate, Action callback, bool isNeedStopCallback = false, string groupTag = "", bool IsReplaceExists = true)
    {
        SGTimeBean bean = null;
        if (IsReplaceExists)
        {
            bean = GenTimerBean(timerName, delayTimeOrFrameCounter, repeatRate, callback, isNeedStopCallback, groupTag);
            _inst.Add(bean);
        }
        else
        {
            bean = _inst.Get(timerName);
            if (bean == null)
            {
                bean = GenTimerBean(timerName, delayTimeOrFrameCounter, repeatRate, callback, isNeedStopCallback, groupTag);
                _inst.Add(bean);
            }
        }
        return bean;
    }

    /// <summary>
    /// 自动管理定时器，用完自动回收，无需名称
    /// </summary>
    public SGTimeBean StartTimer(float delayTimeOrFrameCounter, int repeatRate, Action callback, bool isNeedStopCallback = false, string groupTag = "", bool IsReplaceExists = true)
    {
        return StartTimer(null, delayTimeOrFrameCounter, repeatRate, callback, isNeedStopCallback, groupTag, IsReplaceExists);
    }

    /// <summary>
    /// 生成一个新定时器
    /// </summary>
    /// <param name="timerName"></param>
    /// <param name="delayTimeOrFrameCounter"></param>
    /// <param name="repeatRate"></param>
    /// <param name="callback"></param>
    /// <param name="isNeedStopCallback"></param>
    /// <param name="groupTag"></param>
    /// <param name="IsReplaceExits"></param>
    /// <returns></returns>
    private SGTimeBean GenTimerBean(string timerName, float delayTimeOrFrameCounter, int repeatRate, Action callback, bool isNeedStopCallback = false, string groupTag = "")
    {
        SGTimeBean bean = timerPool.GetObject();
        bean.Init();
        bean.timerName = timerName;
        bean.DelayTime = delayTimeOrFrameCounter;
        bean.repeatRate = repeatRate;
        bean.callback = callback;
        bean.groupTag = groupTag;
        bean.isNeedStopCallback = isNeedStopCallback;
        return bean;
    }

    /// <summary>
    /// 启用一个mono Update
    /// </summary>
    /// <param name="callback"></param>
    /// <returns></returns>
    public SGTimeBean StartUpdate(string timerName, Action callback)
    {
        return this.StartTimer(timerName, 0, -1, callback);
    }

    /// <summary>
    /// 启动帧率刷新
    /// </summary>
    /// <param name="timerName">帧率刷新名称</param>
    /// <param name="framCount">帧率正数</param>
    /// <param name="callback">回调</param>
    /// <returns></returns>
    public SGTimeBean StartFrameUpdate(string timerName, uint framCount, Action callback)
    {
        return this.StartTimer(timerName, -framCount, -1, callback);
    }

    /// <summary>
    /// 启动一个协程
    /// </summary>
    /// <param name="routine"></param>
    /// <returns></returns>
    public Coroutine StartCoroutine(IEnumerator routine)
    {
        return GetMono().StartCoroutine(routine);
    }

    /// <summary>
    /// 停止一个协程
    /// </summary>
    /// <param name="routine"></param>
    public void StopCoroutine(IEnumerator routine)
    {
        GetMono().StopCoroutine(routine);
    }

    /// <summary>
    /// 清除所有
    /// </summary>
    public void StopAll()
    {
        _inst.Clear();
    }

    /// <summary>
    /// 停止定时
    /// </summary>
    /// <param name="timerName">定时器名称</param>
    public void StopTimer(string timerName)
    {
        _inst.Remove(timerName);
    }

    /// <summary>
    /// 停止定时
    /// </summary>
    /// <param name="timerName">定时器名称</param>
    /// <param name="isNeedStopCallback">是否结束后回调</param>
    public void StopTimer(string timerName, bool isNeedStopCallback)
    {
        _inst.Remove(timerName, isNeedStopCallback);
    }

    public SGTimeMono GetMono()
    {
        return _inst;
    }

    #endregion 公共处理

    /// <summary>
    /// 清除所有定时器实例
    /// </summary>
    public void Destroy()
    {
        _inst.Clear();
        GameObject.DestroyImmediate(_inst.gameObject);
    }

    /// <summary>
    /// 获取当前执行的时间数量
    /// </summary>
    /// <returns></returns>
    public int GetTimeCount()
    {
        return _inst.timerCount;
    }
}

/// <summary>
/// 定时器缓存数据包
/// </summary>
public class SGTimeBean : ITimerReference
{
    public float delayTime;
    public int frameCounter = 0;
    public bool isRemoved = false;
    public int _loopCount = 0;
    public double _oldTime = 0;
    public bool isLoop = false;

    public Action callback;
    public string groupTag = "";
    public bool isNeedStopCallback = false;
    public string timerName;
    public Action onComplete;
    public TimerModel timerModel = TimerModel.Time;
    /// <summary>
    /// 定时器刷新类型
    /// </summary>
    public enum TimerModel
    {
        Time, //时间
        Frame, //帧
    }
    public void Init()
    {
        delayTime = 0;
        frameCounter = 0;
        groupTag = "";
        isNeedStopCallback = false;
        IsRemoved = false;
        timerName = "";
        _loopCount = 0;
        callback = null;
        onComplete = null;
        isLoop = false;

        _oldTime = GetNowSpam();
    }

    public int repeatRate
    {
        set
        {
            _loopCount = value;
            if (_loopCount == -1)
            {
                isLoop = true;
            }
        }
    }

    public float DelayTime
    {
        get
        {
            return delayTime;
        }
        set
        {
            if (value > 0)
            {
                delayTime = value * 10000000;
                timerModel = TimerModel.Time;
            }
            else
            {
                delayTime = value;
                timerModel = TimerModel.Frame;
                frameCounter = (int)(-DelayTime); //设置帧率
            }
        }
    }

    public bool IsRemoved
    {
        get => isRemoved; set
        {
            isRemoved = value;
        }
    }

    private bool isRecycled;
    public bool IsRecycled { get => isRecycled; set => isRecycled = value; }

    double GetNowSpam()
    {
        return DateTime.Now.Ticks;
    }

    /// <summary>
    /// 执行回调
    /// </summary>
    public void InvokeCallback()
    {
        callback?.Invoke();
    }

    /// <summary>
    /// 执行完毕回调事件
    /// </summary>
    public void InvokeComplete()
    {
        onComplete?.Invoke();
    }

    /// <summary>
    /// 执行完毕回调事件
    /// </summary>
    /// <param name="action"></param>
    public void OnComplete(Action action)
    {
        onComplete = action;
    }

    public void Update()
    {
        if (_oldTime != -1)
        {
            Function();
        }
    }

    private void Function()
    {
        if (timerModel == TimerModel.Time)
        {
            TimerUpdate();
        }
        else
        {
            FramUpdate();
        }
    }

    /// <summary>
    /// 帧刷新器
    /// </summary>
    public void FramUpdate()
    {
        if (frameCounter-- > 0)
        {
            return;
        }
        else
        {
            //删除后是否需要回调处理
            if (isNeedStopCallback && IsRemoved)
            {
                InvokeCallback();
                InvokeComplete();
            }
            else if (!IsRemoved)
            {
                InvokeCallback();
            }

            // 循环处理
            if (isLoop)
            {
                frameCounter = (int)(-DelayTime);
            }
            else
            {
                if (--_loopCount > 0)
                {
                    frameCounter = (int)(-DelayTime);
                }
                else
                {
                    if (frameCounter <= 0)
                    {
                        IsRemoved = true;
                        InvokeComplete();
                        _oldTime = -1;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 时间刷新器
    /// </summary>
    public void TimerUpdate()
    {
        if (GetNowSpam() > _oldTime + DelayTime)
        {
            //删除后是否需要回调处理
            if (isNeedStopCallback && IsRemoved)
            {
                InvokeCallback();
                InvokeComplete();
            }
            else if (!IsRemoved)
            {
                InvokeCallback();
            }

            // 循环处理
            if (isLoop)
            {
                _oldTime = _oldTime + DelayTime;
            }
            else
            {
                if (--_loopCount > 0)
                {
                    _oldTime = _oldTime + DelayTime;
                }
                else
                {
                    if (GetNowSpam() > _oldTime + DelayTime)
                    {
                        IsRemoved = true;
                        InvokeComplete();
                        _oldTime = -1;
                    }
                }
            }
        }
    }


    public void Clear()
    {
        delayTime = 0;
        frameCounter = 0;
        groupTag = "";
        isNeedStopCallback = false;
        //清理时候是删除状态
        IsRemoved = true;
        timerName = "";
        _loopCount = 0;

        callback = null;
        onComplete = null;

        isLoop = false;
        _oldTime = -1;
    }

    /// <summary>
    /// 停止时间
    /// </summary>
    public void Stop() {
        SGTimer.Stop(timerName);
    }
}

public class SGTimeMono : MonoBehaviour
{
    #region 内部处理

    public ConcurrentDictionary<string, SGTimeBean> timeList = new ConcurrentDictionary<string, SGTimeBean>();
    // Update is called once per frame
    private long count = 0;
    [ReadOnly]
    public int timerCount = 0;
    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="bean"></param>
    public void Add(SGTimeBean bean)
    {
        count++;
        if (count == long.MaxValue)
        {
            count = 0;
        }
        if (string.IsNullOrEmpty(bean.timerName))
        {
            bean.timerName = $"{DateTime.Now.Ticks}-{count}-{bean.GetHashCode()}";
        }
        if (timeList.ContainsKey(bean.timerName))
        {
            Remove(bean.timerName);
        }
        timeList.TryAdd(bean.timerName, bean);
    }

    /// <summary>
    /// 获取时间，空名称不支持
    /// </summary>
    /// <param name="timerName"></param>
    /// <returns></returns>
    public SGTimeBean Get(string timerName)
    {
        if (string.IsNullOrEmpty(timerName))
        {
            return null;
        }
        if (timeList.ContainsKey(timerName))
        {
            return timeList[timerName];
        }
        return null;
    }

    public void Clear()
    {
        List<string> keys = new List<string>(timeList.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            if (timeList.ContainsKey(keys[i]))
            {
                SGTimeBean bean = timeList[keys[i]];
                bean.IsRemoved = true;
                RemoveFromList(keys[i]);
            }
        }
        timeList.Clear();
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="timerName"></param>
    /// <param name="isNeedStopCallback"></param>
    public void Remove(string timerName, bool isNeedStopCallback)
    {
        if (timeList.ContainsKey(timerName))
        {
            SGTimeBean bean = timeList[timerName];
            bean.IsRemoved = true;
            bean.isNeedStopCallback = isNeedStopCallback;
            if (bean.isNeedStopCallback)
            {
                bean.InvokeCallback();
                bean.InvokeComplete();
            }
            RemoveFromList(timerName);
        }
    }

    public void Remove(string timerName)
    {
        if (timeList.ContainsKey(timerName))
        {
            SGTimeBean bean = timeList[timerName];
            bean.IsRemoved = true;
            if (bean.isNeedStopCallback)
            {
                bean.InvokeCallback();
                bean.InvokeComplete();
            }
            RemoveFromList(timerName);
        }
    }

    /// <summary>
    /// 删除所有，isNeedStopCallback 有效
    /// </summary>
    public void RemoveAll()
    {
        List<string> keys = new List<string>(timeList.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            if (timeList.ContainsKey(keys[i]))
            {
                SGTimeBean bean = timeList[keys[i]];
                bean.IsRemoved = true;
                if (bean.isNeedStopCallback)
                {
                    bean.InvokeCallback();
                    bean.InvokeComplete();
                }
                RemoveFromList(keys[i]);
            }
        }
    }

    /// <summary>
    /// 按照相同标签删除
    /// </summary>
    /// <param name="timerName"></param>
    /// <param name="isNeedStopCallback"></param>
    public void RemoveByGroupTag(string groupTag, bool isNeedStopCallback = false)
    {
        List<string> keys = new List<string>(timeList.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            string key = keys[i];
            if (timeList.ContainsKey(key) && timeList[key].groupTag == groupTag)
            {
                Remove(key, isNeedStopCallback);
            }
        }
    }

    private void RemoveFromList(string timerName)
    {
        //Debug.LogError("定时器-开始移除！" + timerName);
        if (timeList.ContainsKey(timerName))
        {
            SGTimeBean bean;
            timeList.TryRemove(timerName, out bean);
            if (bean != null)
            {
                //Debug.LogError("定时器-移除：" + (bean).timerName + "|" + (bean).IsRemoved + "|" + (bean).completeCallback+"|"+ timeList.ContainsKey(timerName));
                SGTimer.timerPool.PutObject(bean);
            }
        }
    }

    private void Update()
    {
        //timerCount = timeList.Count;
        timerCount = SGTimer.timerPool.Count;
        if (timeList.Count <= 0)
        {
            return;
        }
        foreach (var item in timeList)
        {
            if (!item.Value.isRemoved && !item.Value.IsRecycled)
            {
                item.Value.Update();
                if (item.Value.isRemoved)
                {
                    RemoveFromList(item.Key);
                }
            }
        }
    }

    #endregion 内部处理
}

public sealed class TimerPool<T> where T : ITimerReference, new()
{
    ConcurrentQueue<T> _objects;
    Func<T> createFunc;
    Action<T> resetFunc;

    public TimerPool(Func<T> createFunc, Action<T> resetFunc, int capacity)
    {
        Contract.Assume(createFunc != null);
        Contract.Assume(capacity >= 0);

        this._objects = new ConcurrentQueue<T>();
        this.createFunc = createFunc;
        this.resetFunc = resetFunc;
        this.Capacity = capacity;
    }

    public int Capacity { get; private set; }
    public int Count { get { return _objects.Count; } }

    /// <summary>
    /// 申请对象
    /// </summary>
    public T GetObject()
    {
        if (_objects.TryDequeue(out T obj))
        {
            obj.IsRecycled = false;
            return obj;
        }
        else
        {
            return createFunc();
        }
    }

    /// <summary>
    /// 释放对象
    /// </summary>
    public void PutObject(T obj)
    {
        //Contract.Assume(obj != null);
        if (Count >= Capacity || obj.IsRecycled)
        {
            return;
        }
        obj.IsRecycled = true;
        resetFunc?.Invoke(obj);
        _objects.Enqueue(obj);
    }
}

public interface ITimerReference
{
    bool IsRecycled { get; set; }

    void Clear();
}