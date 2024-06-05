using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameScene : UI_Scene
{
    public GameObject NameObject;
    protected Text _nameText;

    public GameObject HpObject;
    protected Image _hpImage;

#if !UNITY_SERVER
    public override void Init()
	{
        base.Init();

        _nameText = NameObject.GetComponent<Text>();
        _hpImage = HpObject.GetComponent<Image>();
    }

    public override void Update()
    {
        base.Update();

        if(_nameText)
            _nameText.text = Managers.Network.AccountName;

        CharacterController co = Managers.Controller.MyController;
        if(_hpImage && co && co._health)
            _hpImage.fillAmount = co._health.CurHpRatio;
    }
#endif
}
