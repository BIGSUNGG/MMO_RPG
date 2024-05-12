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

        _animator.SetBool("DoNextCombo", _knight._bDoNextCombo);
    }

    protected virtual void OnComboStartEvent()
    {
        _animator.SetTrigger("OnComboStart");
    }

    protected virtual void OnComboEndEvent()
    {
    }

}
