using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.UI.GridLayoutGroup;

public class UI_ItemShop_Item : UI_Popup
{
    enum Texts
    {
        Price,
        Name,
    }

    enum Images
    {
        ItemIcon,
        SelectBtn,
        SelectedImg,
    }

    protected Image SelectedImg;
    protected Image IconImg;
    protected Text PriceText;
    protected Text NameText;

    UI_ItemShop _owner;
    ProductInfo _itemInfo;
    int _itemIndex = 0;

    public override void Init()
    {
        base.Init();

        Bind<Text>(typeof(Texts));
        Bind<Image>(typeof(Images));

        _owner = gameObject.GetComponentInParent<UI_ItemShop>();

        IconImg = GetImage((int)Images.ItemIcon);
        if (IconImg == null)
            Debug.LogWarning("Icon Image is not exist");

        GetImage((int)Images.SelectBtn).gameObject.BindEvent(OnClickSelectButton);

        SelectedImg = GetImage((int)Images.SelectedImg);
        if (SelectedImg == null)
            Debug.LogWarning("Selected Image is not exist");

        PriceText = GetText((int)Texts.Price);
        if (PriceText == null)
            Debug.LogWarning("Price Text is not exist");

        NameText = GetText((int)Texts.Name);
        if (NameText == null)
            Debug.LogWarning("Name Text is not exist");
    }

    public void Update()
    {
        if(_owner.SelectedItem == _itemIndex) // 선택한 아이템이 현재 슬롯일 경우
            SelectedImg.enabled = true;
        else
            SelectedImg.enabled = false;

    }

    public void SetInfo(ProductInfo info, int index)
    {
        _itemInfo = info;
        _itemIndex = index;
    }

    protected void OnClickSelectButton(PointerEventData evt)
    {
        _owner.SelectedItem = _itemIndex;
    }

    public void RefreshUI()
    {
        if (_itemInfo == null)
            return;
        
        Item item = Item.FindItem(_itemInfo.itemType);
        if (item == null)
            return;

        Sprite itemIcon = Resources.Load<Sprite>(item.IconImagePath);
        IconImg.sprite = itemIcon;

        PriceText.text = _itemInfo.price.ToString() + " Gold";
        NameText.text = item.ItemName;
    }
}
