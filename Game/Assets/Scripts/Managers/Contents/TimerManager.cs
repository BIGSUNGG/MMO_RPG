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
        List<TimerHandler> newTimers = new List<TimerHandler>(); // ���� Ÿ�̸Ӱ� ������ ���� Ÿ�̸ӵ��� ���� ����Ʈ

        lock (_lock)
        {
	        foreach (var timer in _timers)
	        {
	            timer._remainTime -= deltaTime;
	            if (timer._remainTime <= 0.0f) // Ÿ�̸Ӱ� �����ٸ�
	            {
	                timer._action.Invoke(); // �׼� ����
	
	                if(timer._bLoop == true) // �ݺ��Ǵ� Ÿ�̸Ӷ��
	                    newTimers.Add(timer); // Ÿ�̸ӿ� �����
	
	            }
	            else // ���� �ð��� ���Ҵٸ�
	            {
	                newTimers.Add(timer); // Ÿ�̸ӿ� �����
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
