using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingDamage : Effect
{
    protected override void Start()
    {
        base.Start();

        Managers.Timer.SetTimer(1.0f, () => { Destroy(this.gameObject); }, false); // 1���� ����Ʈ ����
    }

    protected override void Update()
    {
        base.Update();

    }    
}
