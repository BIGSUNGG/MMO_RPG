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
                if (timer._remainTime <= 0.0f) // Ÿ�̸Ӱ� �����ٸ�
                {
                    finishTimers.Add(timer); // �׼� �߰�	
                }
                else // ���� �ð��� ���Ҵٸ�
                {
                }
            }
        }

        foreach (var timer in finishTimers)
        {
            if (timer._bLoop == false) // �ݺ������ʴ� Ÿ�̸Ӷ��
                RemoveTimer(timer); // Ÿ�̸� ����

            timer._action.Invoke(); // �׼� ����
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
