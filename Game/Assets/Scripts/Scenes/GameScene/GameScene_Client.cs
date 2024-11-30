using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if !UNITY_SERVER
public partial class GameScene
{
    UI_GameScene _sceneUI;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Game;
        Screen.SetResolution(640, 480, false);
        _sceneUI = Managers.UI.ShowSceneUI<UI_GameScene>();
    }

    public override void Clear()
    {
        
    }
}
#endif
