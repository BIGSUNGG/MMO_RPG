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

        List<Action> actions = new List<Action>();
        lock (_lock)
        {
            List<TimerHandler> newTimers = new List<TimerHandler>(_timers.Count); // ���� Ÿ�̸Ӱ� ������ ���� Ÿ�̸ӵ��� ���� ����Ʈ

            foreach (var timer in _timers)
	        {
                if (timer == null)
                    continue;

	            timer._remainTime -= deltaTime;
	            if (timer._remainTime <= 0.0f) // Ÿ�̸Ӱ� �����ٸ�
	            {
                    actions.Add(timer._action); // �׼� �߰�
	
	                if(timer._bLoop == true) // �ݺ��Ǵ� Ÿ�̸Ӷ��
	                    newTimers.Add(timer); // Ÿ�̸ӿ� �����
	
	            }
	            else // ���� �ð��� ���Ҵٸ�
	            {
	                newTimers.Add(timer); // Ÿ�̸ӿ� �����
	            }
	        }        
            
            _timers = newTimers;

        }

        foreach (var action in actions)
        {
            action.Invoke(); // �׼� ����
        }
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
