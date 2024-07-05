using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IPotion : Item
{
    public IPotion()
    {
        _itemType = ItemType.SmallPotion;
        _iconImgName = "Potion";
    }

    #region Use
    protected int _healPoint = 25;

    public override bool OnServer_Use(ObjectController oc)
    {
        if (Util.CheckFuncCalledOnServer() == false)
            return false;

        CharacterController cc = oc as CharacterController;
        if (cc == null)
            return false;

        cc.Health.IncreaseHp(_healPoint);
        return true;
    }
    #endregion
}
