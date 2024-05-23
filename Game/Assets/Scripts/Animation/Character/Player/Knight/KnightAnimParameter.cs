using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightAnimParameter : PlayerAnimParameter
{
    KnightController _knight = new KnightController();

    protected override void Start()
    {
        base.Start();

        _knight = GetComponent<KnightController>();
        _knight._onComboStartEvent.AddListener(OnComboStartEvent);
        _knight._onComboEndEvent.AddListener(OnComboEndEvent);

    }

    protected override void Update()
    {
        base.Update();

        _animator.SetInteger("Current Combo", _knight._curCombo);
    }

    protected virtual void OnComboStartEvent()
    {
        _animator.SetTrigger("On Combo Start");
        _animator.ResetTrigger("On Combo End");
    }

    protected virtual void OnComboEndEvent()
    {
        _animator.SetTrigger("On Combo End");
        _animator.ResetTrigger("On Combo Start");
    }

}
