using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if !UNITY_SERVER
public partial class LoginScene
{
    UI_LoginScene _sceneUI;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Login;
               
        Managers.Web.BaseUrl = "https://localhost:5001/api";
        _sceneUI = Managers.UI.ShowSceneUI<UI_LoginScene>();
    }

    public override void Clear()
    {
        
    }
}
#endif
