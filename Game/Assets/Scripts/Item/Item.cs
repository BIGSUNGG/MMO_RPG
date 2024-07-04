using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item
{
    public ItemType _itemType { get; protected set; }

    #region Use
    public abstract bool OnServer_Use(ObjectController oc);
    #endregion

    #region Factory
    static Item[] _factory = 
    {
        null,
        new Potion()
    };

    static public Item FindItem(ItemType type) { return _factory[(byte)type]; }
    #endregion
}
