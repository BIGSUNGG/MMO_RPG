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

    public GameObject MoneyObject;
    protected Text _moneyText;


#if !UNITY_SERVER
    public override void Init()
	{
        base.Init();

        _nameText   = NameObject.GetComponent<Text>();
        _hpImage    = HpObject.GetComponent<Image>();
        _moneyText  = MoneyObject.GetComponent<Text>();
    }

    public override void Update()
    {
        base.Update();

        PlayerController pc = Managers.Controller.MyController;
        if (pc == null)
            return;

        if(_nameText)
            _nameText.text = Managers.Network.AccountName;

        if(_hpImage  && pc.Health)
            _hpImage.fillAmount = pc.Health.CurHpRatio;

        if(_moneyText && pc.Inventory)
            _moneyText.text = pc.Inventory._money.ToString() + " Gold";
    }
#endif
}
