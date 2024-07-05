using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallPotion : IPotion
{
    public SmallPotion()
    {
        _itemName = "Small Potion";
        _itemType = ItemType.SmallPotion;
        _iconImgName = "SmallPotion";
        _healPoint = 25;
    }
}
