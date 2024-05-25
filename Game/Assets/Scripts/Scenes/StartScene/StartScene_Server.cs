using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_SERVER
public partial class StartScene
{
    protected override void Init()
    {
        Application.targetFrameRate = 1000;
        Managers.Network.ConnectToServer();
    }

    public override void Clear()
    {
        
    }
}
#endif
