using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameScene : UI_Scene
{
    enum GameObjects
    {
        ItemSlot,
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

    protected List<Image> _itemSlotImages = new List<Image>(9);
    protected List<Text> _itemSlotTexts = new List<Text>(9);

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

        GameObject itemSlot = Get<GameObject>((int)GameObjects.ItemSlot);
        foreach(var image in itemSlot.GetComponentsInChildren<Image>())
        {
            if (image.gameObject.name == "IconImage")
                _itemSlotImages.Add(image);
        }
        foreach (var text in itemSlot.GetComponentsInChildren<Text>())
        {
            if (text.gameObject.name == "Count")
                _itemSlotTexts.Add(text);
        }

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

        RefreshItemSlot();
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

    protected virtual void RefreshItemSlot()
    {
        PlayerController pc = Managers.Controller.MyController;
        if (pc == null)
            return;

        // 슬롯 이미지 초기화
        for (int i = 0; i < _itemSlotImages.Count; i++)
        {
            _itemSlotImages[i].sprite = null;
            _itemSlotImages[i].enabled = false;

            _itemSlotTexts[i].enabled = false;
        }

        // 슬롯 이미지 설정
        List<ItemInfo> items = pc.Inventory.ItemSlot;
        for (int i = 0; i < items.Count; i++)
        {
            Item item = Item.FindItem(items[i].Type);
            if (item == null)
                continue;
   
            Sprite itemIcon = Resources.Load<Sprite>(item.IconImagePath);
            _itemSlotImages[i].sprite = itemIcon;
            _itemSlotImages[i].enabled = true;

            _itemSlotTexts[i].text = items[i].Count.ToString();
            _itemSlotTexts[i].enabled = true;
        }
    }
    #endif
}
