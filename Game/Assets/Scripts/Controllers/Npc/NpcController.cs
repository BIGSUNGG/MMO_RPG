using Google.Protobuf;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ProductInfo
{
    public ItemType itemType;
    public int price;
}

public class NpcController : ObjectController
{
    public NpcController()
    {
        ObjectType = GameObjectType.Npc;
    }

    protected override void Start()
    {
        base.Start();

        {
            ProductInfo info = new ProductInfo();
            info.itemType = ItemType.SmallPotion;
            info.price = 25;
            _products.Add(info);
        }
        {
            ProductInfo info = new ProductInfo();
            info.itemType = ItemType.NormalPotion;
            info.price = 40;
            _products.Add(info);
        }
        {
            ProductInfo info = new ProductInfo();
            info.itemType = ItemType.BigPotion;
            info.price = 75;
            _products.Add(info);
        }
    }

    protected override void Update()
    {
        base.Update();

    }

    #region Npc
    public List<ProductInfo> Products { get { return _products; } }
    protected List<ProductInfo> _products = new List<ProductInfo>();
    #endregion

    #region Sync
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    protected class NpcSyncInfo : ObjectSyncInfo
    {
        // Movement
        public Vector3 position;
        public Quaternion rotation;
    }

    public override void ObjectSync(ByteString syncInfo)
    {
        NpcSyncInfo info = Util.BytesToObject<NpcSyncInfo>(syncInfo.ToByteArray());
        ObjectSync(info);
    }

    protected void ObjectSync(NpcSyncInfo info)
    {
        if (info == null)
            return;

        if (!IsLocallyControlled())
        {
            transform.position = info.position;
            transform.rotation = info.rotation;
        }

        base.ObjectSync(info);
    }

    public override ByteString GetObjectSyncInfo()
    {
        NpcSyncInfo info = new NpcSyncInfo();
        GetObjectSyncInfo(info);

        return ByteString.CopyFrom(Util.ObjectToBytes<NpcSyncInfo>(info));
    }

    protected void GetObjectSyncInfo(NpcSyncInfo info)
    {
        if (info == null)
            return;

        info.position = transform.position;
        info.rotation = transform.rotation;

        base.GetObjectSyncInfo(info);
    }
    #endregion
}
