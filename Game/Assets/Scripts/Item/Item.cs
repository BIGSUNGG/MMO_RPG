using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item
{

    #region Item
    public ItemType _itemType { get; protected set; }
    public string ItemName { get { return _itemName; } }
    protected string _itemName;

    public string IconImagePath { get { return "Icon/" + _iconImgName; } }
    protected string _iconImgName;
    #endregion

    #region Use
    public abstract bool OnServer_Use(ObjectController oc);
    #endregion

    #region Factory
    static Item[] Items = 
    {
        null,
        new SmallPotion(),
        new NormalPotion(),
        new BigPotion(),
    };

    static public Item FindItem(ItemType type) { return Items[(byte)type]; }
    #endregion
}
