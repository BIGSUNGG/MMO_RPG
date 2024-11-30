using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if !UNITY_SERVER
public class UI_Scene : UI_Base
{
	public override void Init()
	{
		Managers.UI.SetCanvas(gameObject, false);
	}

    public virtual void Update()
    {

    }
}
#else
public class UI_Scene : UI_Base
{
    public override void Init()
    {
    }
}
#endif
