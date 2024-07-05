using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ItemShop_Item : UI_Popup
{
    public GameObject IconObject;
    protected Image IconImg;
    public GameObject PriceObject;
    protected Text PriceText;
    public GameObject NameObject;
    protected Text NameText;

    public ProductInfo Info;

    public override void Init()
    {
        base.Init();

        IconImg = IconObject.GetComponent<Image>();
        if (IconImg == null)
            Debug.LogWarning("Icon Image is not exist");

        PriceText = PriceObject.GetComponent<Text>();
        if (PriceText == null)
            Debug.LogWarning("Price Text is not exist");

        NameText = NameObject.GetComponent<Text>();
        if (NameText == null)
            Debug.LogWarning("Name Text is not exist");
    }

    public void RefreshUI()
    {
        if (Info == null)
            return;
        
        Item item = Item.FindItem(Info.itemType);
        if (item == null)
            return;

        Sprite itemIcon = Resources.Load<Sprite>(item.IconImagePath);
        IconImg.sprite = itemIcon;

        PriceText.text = Info.price.ToString() + " Gold";
        NameText.text = item.ItemName;
    }
}
