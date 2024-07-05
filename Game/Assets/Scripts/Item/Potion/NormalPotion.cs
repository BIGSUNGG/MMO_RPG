using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalPotion : IPotion
{
    public NormalPotion()
    {
        _itemName = "Normal Potion";
        _itemType = ItemType.NormalPotion;
        _iconImgName = "NormalPotion";
        _healPoint = 50;
    }
}
