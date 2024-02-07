using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

#if UNITY_SERVER
public partial class GameScene
{
    protected override void Init()
    {
        Application.targetFrameRate = 30;

        Managers.Network.ConnectToServer();
    }

    public override void Clear()
    {
    }
}
#endif
