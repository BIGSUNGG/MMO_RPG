using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryComponent: ObjectComponent
{

    protected override void Start()
    {
    }

    protected override void Update()
    {
        
    }

    #region Inventory
    public Weapon _curWeapon { get; protected set; } = null;

    public virtual void EquipWeapon(Weapon equipWeapon)
    {
        _curWeapon = equipWeapon;
    }

    #endregion
}
