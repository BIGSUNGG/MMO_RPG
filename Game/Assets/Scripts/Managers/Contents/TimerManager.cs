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
    List<TimerHandler> _timers = new List<TimerHandler>();

    public void Update()
    {
        float deltaTime = Time.deltaTime;
        List<TimerHandler> newTimers = new List<TimerHandler>(); // 아직 타이머가 끝나지 않은 타이머들을 가질 리스트

        lock (_lock)
        {
	        foreach (var timer in _timers)
	        {
	            timer._remainTime -= deltaTime;
	            if (timer._remainTime <= 0.0f) // 타이머가 끝났다면
	            {
	                timer._action.Invoke(); // 액션 실행
	
	                if(timer._bLoop == true) // 반복되는 타이머라면
	                    newTimers.Add(timer); // 타이머에 남기기
	
	            }
	            else // 아직 시간이 남았다면
	            {
	                newTimers.Add(timer); // 타이머에 남기기
	            }
	        }
        }

        _timers = newTimers;
    }

    #region Timer
    public TimerHandler SetTimer(float timerTime, Action action, bool bLoop = false)
    {
        lock (_lock)
        {
            TimerHandler result = new TimerHandler(timerTime, action, bLoop);
            _timers.Add(result);
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

    #endregion
}
