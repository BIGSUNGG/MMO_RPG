using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if !UNITY_SERVER
public class UI_GameScene : UI_Scene
{
    public UI_Stat StatUI { get; private set; }

    public override void Init()
	{
        base.Init();

        StatUI = GetComponentInChildren<UI_Stat>();

        StatUI.gameObject.SetActive(false);
	}
}
#endif
