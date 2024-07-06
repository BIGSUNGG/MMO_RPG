using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ItemShop : UI_Popup
{
    enum Images
    {
        BuyBtn,
    }

    public List<UI_ItemShop_Item> Items { get; } = new List<UI_ItemShop_Item>();
    protected NpcController _dealer;

    public int SelectedItem { get { return _selectedItem; } set { _selectedItem = value; } }
    protected int _selectedItem = 0;

    public override void Init()
    {
        base.Init();

        Bind<Image>(typeof(Images));

        GetImage((int)Images.BuyBtn).gameObject.BindEvent(OnClickBuyButton);
    }

    public void Update()
    {
        
    }

    protected void OnClickBuyButton(PointerEventData evt)
    {
        PlayerController pc = Managers.Controller.MyController;

        if (pc == null || _dealer == null)
            return;

        pc.Inventory.PurchaseItem(_dealer, _selectedItem);
    }

    public void SetDealer(NpcController dealer)
    {
        _dealer = dealer;

        GameObject grid = GetComponentInChildren<GridLayoutGroup>().gameObject;

        foreach (var item in Items)
            Managers.Resource.Destroy(item.gameObject);

        Items.Clear();

        for (int i = 0; i < dealer.Products.Count; i++)
        {
            GameObject go = Managers.Resource.Instantiate("UI/Popup/UI_ItemShop_Item", grid.transform);
            RectTransform rectTransform = go.transform as RectTransform;
            rectTransform.localScale = Vector3.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            UI_ItemShop_Item item = go.GetOrAddComponent<UI_ItemShop_Item>();

            Items.Add(item);
            item.SetInfo(dealer.Products[i], i);
        }

        RefreshUI();
    }

    public void RefreshUI()
    {
        if (Items.Count == 0)
            return;

        foreach (var item in Items)
        {
            item.RefreshUI();
        }
    }
}