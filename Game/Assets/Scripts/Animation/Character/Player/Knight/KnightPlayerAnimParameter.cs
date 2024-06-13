using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightPlayerAnimParameter : PlayerAnimParameter
{
    KnightPlayerController _knight = new KnightPlayerController();

    protected override void Start()
    {
        base.Start();

        _knight = GetComponent<KnightPlayerController>();
    }

    protected override void Update()
    {
        base.Update();

    }

}
