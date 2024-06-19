using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

delegate void ActionDele();

public class TimerHandler
{
    public TimerHandler(float timerTime, Action action, bool bLoop)
    {
        _remainTime = timerTime;
        _action = action;
        _bLoop = bLoop;
    }

    public float _remainTime = 0.0f;
    public Action _action;
    public bool _bLoop;
}

public class TimerManager
{
    object _lock = new object();
    LinkedList<TimerHandler> _timers = new LinkedList<TimerHandler>();

    public void Update()
    {
        float deltaTime = Time.deltaTime;

        List<TimerHandler> finishTimers = new List<TimerHandler>();
        lock (_lock)
        {
            foreach (var timer in _timers)
            {
                if (timer == null)
                    continue;

                timer._remainTime -= deltaTime;
                if (timer._remainTime <= 0.0f) // 타이머가 끝났다면
                {
                    finishTimers.Add(timer); // 액션 추가	
                }
                else // 아직 시간이 남았다면
                {
                }
            }
        }

        foreach (var timer in finishTimers)
        {
            if (timer._bLoop == false) // 반복되지않는 타이머라면
                RemoveTimer(timer); // 타이머 제거

            timer._action.Invoke(); // 액션 실행
        }
    }

    #region Timer
    public TimerHandler SetTimer(float timerTime, Action action, bool bLoop = false)
    {
        lock (_lock)
        {
            TimerHandler result = new TimerHandler(timerTime, action, bLoop);
            _timers.AddLast(result);
            return result;
        }
    }

    public TimerHandler SetTimerNextTick(Action action)
    {
        return SetTimer(0.0f, action, false);
    }

    public void RemoveTimer(TimerHandler timer)
    {
        lock(_lock)
        {
            _timers.Remove(timer);
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _timers.Clear();
        }
    }

    #endregion
}
