using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ItemShop : UI_Popup
{
    public List<UI_ItemShop_Item> Items { get; } = new List<UI_ItemShop_Item>();

    public void SetDealer(NpcController dealer)
    {
		GameObject grid = GetComponentInChildren<GridLayoutGroup>().gameObject;

        foreach (var item in Items)
            Managers.Resource.Destroy(item.gameObject);

        Items.Clear();
        
        for(int i = 0; i < dealer.Products.Count;  i++)
        {
            GameObject go = Managers.Resource.Instantiate("UI/Popup/UI_ItemShop_Item", grid.transform);
            RectTransform rectTransform = go.transform as RectTransform;
            rectTransform.localScale = Vector3.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            UI_ItemShop_Item item = go.GetOrAddComponent<UI_ItemShop_Item>();

            Items.Add(item);
            item.Info = dealer.Products[i];
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
