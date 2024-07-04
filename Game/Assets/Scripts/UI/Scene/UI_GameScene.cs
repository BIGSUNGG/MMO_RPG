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

    public GameObject MapEnterBlackTransition;
    protected Image _mapEnterTrasitionImage;
    protected Animator _mapEnterTrasitionAnim;

    public GameObject MapLeaveBlackTransition;
    protected Image _mapLeaveTrasitionImage;
    protected Animator _mapLeaveTrasitionAnim;

#if !UNITY_SERVER
    public override void Init()
	{
        base.Init();

        _nameText   = NameObject.GetComponent<Text>();
        _hpImage    = HpObject.GetComponent<Image>();
        _moneyText  = MoneyObject.GetComponent<Text>();

        _mapEnterTrasitionImage = MapEnterBlackTransition.GetComponent<Image>();
        _mapEnterTrasitionAnim = MapEnterBlackTransition.GetComponent<Animator>();

        _mapEnterTrasitionImage.enabled = true;
        _mapEnterTrasitionAnim.enabled = false;

        _mapLeaveTrasitionImage = MapLeaveBlackTransition.GetComponent<Image>();
        _mapLeaveTrasitionAnim = MapLeaveBlackTransition.GetComponent<Animator>();

        _mapLeaveTrasitionImage.enabled = false;
        _mapLeaveTrasitionAnim.enabled = false;

        Managers.Controller._onPossess.AddListener(OnPossess);
        Managers.Controller._onUnpossess.AddListener(OnUnpossess);
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
            _moneyText.text = pc.Inventory.Money.ToString() + " Gold";
    }

    protected virtual void OnPossess()
    {
        _mapEnterTrasitionImage.enabled = true;
        _mapEnterTrasitionAnim.enabled = true;

        _mapLeaveTrasitionImage.enabled = false;
        _mapLeaveTrasitionAnim.enabled = false;
    }

    protected virtual void OnUnpossess()
    {
        _mapEnterTrasitionImage.enabled = false;
        _mapEnterTrasitionAnim.enabled = false;

        _mapLeaveTrasitionImage.enabled = true;
        _mapLeaveTrasitionAnim.enabled = true;
    }
#endif
}
