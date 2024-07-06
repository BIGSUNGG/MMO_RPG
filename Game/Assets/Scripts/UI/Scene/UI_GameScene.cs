using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameScene : UI_Scene
{
    enum GameObjects
    {
        BlackEnterTransition,
        BlackLeaveTransition,
    }

    enum Texts
    { 
        Name,
        Money,
    }

    enum Images
    {
        HP_Fill,
    }

    protected Text _nameText;
    protected Text _moneyText;
    protected Image _hpImage;

    protected Image _mapEnterTrasitionImage;
    protected Animator _mapEnterTrasitionAnim;

    protected Image _mapLeaveTrasitionImage;
    protected Animator _mapLeaveTrasitionAnim;

    #if !UNITY_SERVER
    public override void Init()
	{
        base.Init();

        Bind<GameObject>(typeof(GameObjects));
        Bind<Text>      (typeof(Texts));
        Bind<Image>     (typeof(Images));

        _nameText   = GetText((int)Texts.Name);
        _moneyText  = GetText((int)Texts.Money);
        _hpImage    = GetImage((int)Images.HP_Fill);

        GameObject mapEnterBlackTransition = Get<GameObject>((int)GameObjects.BlackEnterTransition);
        _mapEnterTrasitionImage = mapEnterBlackTransition.GetOrAddComponent<Image>();
        _mapEnterTrasitionAnim = mapEnterBlackTransition.GetOrAddComponent<Animator>();

        _mapEnterTrasitionImage.enabled = true;
        _mapEnterTrasitionAnim.enabled = false;

        GameObject mapLeaveBlackTransition = Get<GameObject>((int)GameObjects.BlackLeaveTransition);
        _mapLeaveTrasitionImage = mapLeaveBlackTransition.GetOrAddComponent<Image>();
        _mapLeaveTrasitionAnim = mapLeaveBlackTransition.GetOrAddComponent<Animator>();

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
