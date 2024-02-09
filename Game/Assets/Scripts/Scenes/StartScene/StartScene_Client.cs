using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if !UNITY_SERVER
public partial class StartScene
{
    UI_LoginScene _sceneUI;

    protected override void Init()
    {
        base.Init();
        SceneManager.LoadScene("Assets/Scenes/Login.unity", LoadSceneMode.Single);

    }

    public override void Clear()
    {
        
    }
}
#endif
