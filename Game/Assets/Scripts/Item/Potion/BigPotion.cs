using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigPotion : IPotion
{
    public BigPotion()
    {
        _itemName = "Big Potion";
        _itemType = ItemType.BigPotion;
        _iconImgName = "BigPotion";
        _healPoint = 100;
    }
}
