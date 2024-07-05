using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using static UnityEngine.Rendering.VolumeComponent;

public class InventoryComponent: ObjectComponent
{
    public InventoryComponent()
    {
        RestItemSlot();
    }

    protected override void Start()
    {
        base.Start();

        HealthComponent health = Owner.GetComponent<HealthComponent>();
        if (health)
            health._onKillEvent.AddListener(OnServer_Kill);
    }

    protected override void Update()
    {
        base.Update();

    }

    protected void OnServer_Kill(ObjectController victim, int damage)
    {
        if (Util.CheckFuncCalledOnServer() == false) // �������� ȣ��������� ���
            return;

        // ������ �÷��̾��� �� �߰�
        PlayerController pc = Owner as PlayerController;
        if (pc == null || pc.Session == null)
            return;

        // �������� HealthComponent ���ϱ�
        HealthComponent victimHealth = victim.GetComponent<HealthComponent>();

        // óġ�� ������ �� �߰�
        pc.Inventory.IncreaseMoney(victimHealth.CalculateKillMoney());
    }

    #region Money
    public int Money { get { return _money; } }
    protected int _money = 0;

    public void SetMoney(int value)
    {
        _money = value;

        if(Managers.Network.IsServer)
            Notify_Money();
    }

    public void IncreaseMoney(int increaseValue)
    {
        _money += increaseValue;

        if(Managers.Network.IsServer)
            Notify_Money();
    }

    public void DecreaseMoney(int decreaseValue)
    {
        _money -= decreaseValue;

        if (Managers.Network.IsServer)
            Notify_Money();
    }

    // ������ Ŭ���̾�Ʈ���� �÷��̾��� �� �˸���
    public void Notify_Money()
    {
        if (Util.CheckFuncCalledOnServer() == false)
            return;

        PlayerController pc = Owner as PlayerController;
        if (pc == null || pc.Session == null)
            return;

        G_NotifyPlayerMoney packet = new G_NotifyPlayerMoney();
        packet.SessionId = pc.Session.SessionId;
        packet.Money = _money;
        Managers.Network.SendServer(packet);
    }

    #endregion

    #region Item
    public List<ItemInfo> ItemSlot { get { return _itemSlot; } }
    public const int ItemSlotSize = 9; // ������ ���� �ִ� ũ��
    List<ItemInfo> _itemSlot = new List<ItemInfo>(new ItemInfo[ItemSlotSize]);

    #region Slot
    protected void RestItemSlot()
    {
        _itemSlot = new List<ItemInfo>(new ItemInfo[ItemSlotSize]);
        for(int i = 0; i < ItemSlotSize; i++)
        {
            _itemSlot[i] = new ItemInfo();
            _itemSlot[i].Type = ItemType.None;
            _itemSlot[i].Count = 0;
        }
    }

    public void SetItemSlot(List<ItemInfo> slot)
    {
        RestItemSlot();

        for (int i = 0; i < slot.Count; i++)
        {
            if (_itemSlot[i] != null)
                _itemSlot[i] = slot[i];
        }

        if (Managers.Network.IsServer)
            Notify_ItemSlotAll();
    }

    public void SetItemSlot(int index, ItemInfo info)
    {
        if (info == null)
        {
            _itemSlot[index].Type = ItemType.None;
            _itemSlot[index].Count = 0;
        }
        else
        {
            _itemSlot[index] = info;
        }

        if (Managers.Network.IsServer)
            Notify_ItemSlot(index);
    }

    // ������ Ÿ�Կ� �´� ���Կ� ������ �߰�
    // itemType : �߰��� �������� Ÿ��
    // return : ������ �߰��� �����ߴ���
    public bool OnServer_AddItem(ItemType itemType)
    {
        if (Util.CheckFuncCalledOnServer() == false)
            return false;

        // ������ ���Կ� �������� �̹� �ִٸ�
        for (int i = 0; i < ItemSlotSize; i++)
        {
            if (_itemSlot[i] != null && _itemSlot[i].Type == itemType)
            {
                // ������ ���� �߰�
                _itemSlot[i].Count++;
                Debug.Log($"Add {itemType}");

                // �������� ���� ���� ��ȭ �˸���
                Notify_ItemSlot(i);

                return true;
            }
        }

        // ������ ���Կ� �������� ���ٸ�
        for (int i = 0; i < ItemSlotSize; i++)
        {
            if (_itemSlot[i] != null && _itemSlot[i].Type != ItemType.None)
                continue;

            // �� ������ ������ �߰�
            _itemSlot[i] = new ItemInfo();
            _itemSlot[i].Type = itemType;
            _itemSlot[i].Count = 1;
            Debug.Log($"Add {itemType}");

            // �������� ���� ���� ��ȭ �˸���
            Notify_ItemSlot(i);

            return true;
        }

        return false;
    }

    // ������ Ÿ�Կ� �´� ���Կ� ������ ����
    // index : �������� ������ ����
    // itemType : ������ �������� Ÿ��
    // return : ������ ���ſ� �����ߴ���
    public bool OnServer_RemoveItem(int index, ItemType itemType)
    {
        if (Util.CheckFuncCalledOnServer() == false)
            return false;

        if (_itemSlot[index] == null || _itemSlot[index].Type == ItemType.None) // ���Կ� �������� ���ٸ�
            return false;

        if (_itemSlot[index].Type == itemType) // ���Կ� �ִ� �������� �����Ϸ��� ����������
        {
            // ������ ���� ����
            _itemSlot[index].Count--;
            Debug.Log($"Remove {itemType}");

            if (_itemSlot[index].Count <= 0) // ������ ������ 0���϶��
            {
                // ������ ���Կ��� ������ ����
                _itemSlot[index].Type = ItemType.None;
                _itemSlot[index].Count = 0;
            }

            // �������� ���� ���� ��ȭ �˸���
            Notify_ItemSlot(index);

            return true;
        }

        return false;
    }

    public void Notify_ItemSlotAll()
    {
        if (Util.CheckFuncCalledOnServer() == false)
            return;

        PlayerController pc = Owner as PlayerController;
        if (pc == null || pc.Session == null)
            return;

        // �������� ���� ���� ��ȭ �˸���
        G_NotifyPlayerItemSlotAll notifyPacket = new G_NotifyPlayerItemSlotAll();
        notifyPacket.SessionId = pc.Session.SessionId;

        foreach (var info in _itemSlot)
        {
            if (info == null)
            {
                ItemInfo itemInfo = new ItemInfo();
                itemInfo.Type = ItemType.None;
                itemInfo.Count = 0;
                notifyPacket.ItemSlot.Add(itemInfo);
            }
            else
            {
                notifyPacket.ItemSlot.Add(info);
            }
        }

        Managers.Network.SendServer(notifyPacket);
    }

    public void Notify_ItemSlot(int index)
    {
        if (Util.CheckFuncCalledOnServer() == false)
            return;

        PlayerController pc = Owner as PlayerController;
        if (pc == null || pc.Session == null)
            return;

        // �������� ���� ���� ��ȭ �˸���
        G_NotifyPlayerItemSlot notifyPacket = new G_NotifyPlayerItemSlot();
        notifyPacket.SessionId = pc.Session.SessionId;
        notifyPacket.Index = index;
        notifyPacket.Info = _itemSlot[index];
        Managers.Network.SendServer(notifyPacket);
    }

    #endregion

    #region Use
    public bool UseItem(int index)
    {
        if (ItemSlotSize <= index || _itemSlot[index] == null || _itemSlot[index].Type == ItemType.None) // ������ ���Կ� ����� �� �ִ� �������� ���ٸ�
        {
            Debug.Log($"Item slot {index} is can't use");
            return false;
        }

        Debug.Log($"Use {index} item");
        Server_UseItem(index, _itemSlot[index].Type);
        return true;
    }

    protected void Server_UseItem(int index, ItemType itemType)
    {
        // ��Ŷ ������
        if (Managers.Network.IsClient) // Ŭ���̾�Ʈ���� ȣ��� ��� 
        {
            C_RpcComponentFunction rpcFuncPacket = new C_RpcComponentFunction();
            byte[] parameterBuffer = new byte[5];
            Array.Copy(BitConverter.GetBytes((int)index), 0, parameterBuffer, 0, sizeof(int));
            Array.Copy(BitConverter.GetBytes((byte)itemType), 0, parameterBuffer, 4, sizeof(byte));

            rpcFuncPacket.ObjectId = Owner.ObjectId;
            rpcFuncPacket.ComponentType = GameComponentType.InventoryComponent;
            rpcFuncPacket.RpcFunctionId = RpcComponentFunctionId.ServerUseItem;
            rpcFuncPacket.ParameterBytes = ByteString.CopyFrom(parameterBuffer);

            Managers.Network.SendServer(rpcFuncPacket);
        }
        else // �������� ȣ��� ���
        {
            Debug.LogError("Client can't call this function");
            return;
        }
    }

    // Rpc ��Ŷ�� ������ ȣ��
    // packet : �Ű������� ����Ʈ �迭 
    protected virtual void Server_UseItem_ReceivePacket(byte[] packet)
    {
        try
        {
            int index = BitConverter.ToInt32(packet, 0);
            byte typeByte = packet[4];
            ItemType itemType = Util.ByteToItemType(typeByte);

            Server_UseItem_Implementation(index, itemType);
        }
        catch (System.Exception ex)
        {
            Debug.Log($"{ex}");
        }
    }

    // �������� ��Ŷ�� �޾��� �� �Ǽ� ��Ŷ�� �����ϱ� ���� ����
    // packet : ���� ��Ŷ�� ����Ʈ �迭
    // return : ���� ��Ŷ�� �Ǽ� ��Ŷ�� �ƴ���
    protected virtual bool Server_UseItem_Validate(byte[] packet)
    {
        try
        {
            int index = BitConverter.ToInt32(packet, 0);
            if (ItemSlotSize <= index)
                return false;

            byte typeByte = packet[4];
            ItemType itemType = Util.ByteToItemType(typeByte);
            if (itemType == ItemType.None)
                return false;

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.Log($"{ex}");
            return false;
        }
    }

    // UseItem �ڵ�
    protected virtual void Server_UseItem_Implementation(int index, ItemType itemType)
    {
        // ����� ������ ����
        bool bSuccessRemove = OnServer_RemoveItem(index, itemType);
        if(bSuccessRemove) // ������ ������ �����ߴٸ� 
        {
            // ������ ���
            Item useItem = Item.FindItem(itemType);
            if(useItem == null)
            {
                Debug.LogError("Try use null item");
                return;
            }

            bool bSuccessUse = useItem.OnServer_Use(Owner);
            if(bSuccessUse)
                Debug.Log($"Use {itemType}");
            else
                Debug.LogError("Failed to use item");
        }
        else
        {
            Debug.LogError("Failed to remove item");
        }
    }
    #endregion

    #region Purchase
    public void PurchaseItem(NpcController dealer, int itemIndex)
    {
        if (dealer.Products.Count <= itemIndex)
        {
            Debug.Log("Select wrong index");
            return;
        }

        ProductInfo info = dealer.Products[itemIndex];
        if (info.price > _money)
        {
            Debug.Log("Need more money");
            return;
        }

        Server_PurchaseItem(dealer, itemIndex);
    }

    protected void Server_PurchaseItem(NpcController dealer, int itemIndex)
    {
        // ��Ŷ ������
        if (Managers.Network.IsClient) // Ŭ���̾�Ʈ���� ȣ��� ��� 
        {
            C_RpcComponentFunction rpcFuncPacket = new C_RpcComponentFunction();
            byte[] parameterBuffer = new byte[8];
            Array.Copy(BitConverter.GetBytes(dealer.ObjectId), 0, parameterBuffer, 0, sizeof(int));
            Array.Copy(BitConverter.GetBytes(itemIndex)      , 0, parameterBuffer, 4, sizeof(int));

            rpcFuncPacket.ObjectId = Owner.ObjectId;
            rpcFuncPacket.ComponentType = GameComponentType.InventoryComponent;
            rpcFuncPacket.RpcFunctionId = RpcComponentFunctionId.ServerPurchaseItem;
            rpcFuncPacket.ParameterBytes = ByteString.CopyFrom(parameterBuffer);

            Managers.Network.SendServer(rpcFuncPacket);
        }
        else // �������� ȣ��� ���
        {
            Debug.LogError("Client can't call this function");
            return;
        }
    }

    // Rpc ��Ŷ�� ������ ȣ��
    // packet : �Ű������� ����Ʈ �迭 
    protected virtual void Server_PurchaseItem_ReceivePacket(byte[] packet)
    {
        try
        {
            int objectId = BitConverter.ToInt32(packet, 0);
            NpcController npc = Managers.Object.FindById(objectId).GetComponent<NpcController>();
            int index = BitConverter.ToInt32(packet, 4);

            Server_PurchaseItem_Implementation(npc, index);
        }
        catch (System.Exception ex)
        {
            Debug.Log($"{ex}");
        }
    }

    // �������� ��Ŷ�� �޾��� �� �Ǽ� ��Ŷ�� �����ϱ� ���� ����
    // packet : ���� ��Ŷ�� ����Ʈ �迭
    // return : ���� ��Ŷ�� �Ǽ� ��Ŷ�� �ƴ���
    protected virtual bool Server_PurchaseItem_Validate(byte[] packet)
    {
        try
        {
            int objectId = BitConverter.ToInt32(packet, 0);
            NpcController npc = Managers.Object.FindById(objectId).GetComponent<NpcController>();
            if (npc == null)
                return false;

            int index = BitConverter.ToInt32(packet, 4);
            if (npc.Products.Count <= index)
                return false;

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.Log($"{ex}");
            return false;
        }
    }

    // Server_PurchaseItem �ڵ�
    protected virtual void Server_PurchaseItem_Implementation(NpcController dealer, int itemIndex)
    {
        if (dealer.Products.Count <= itemIndex)
        {
            Debug.LogError("Select wrong index");
            return;
        }

        ProductInfo info = dealer.Products[itemIndex];
        if (info.price > _money) // ���� ������ ���
        {
            Debug.LogError("Need more money");
            return;
        }

        // �� ���� ���� ������ �߰�
        DecreaseMoney(info.price);
        OnServer_AddItem(info.itemType);
    }
    #endregion

    #endregion

    #region RpcFunction
    // �ٸ� Ŭ���̾�Ʈ�� ��Ŷ�� ������ FunctionId�� �´� �Լ� ȣ��
    // functionId : ���� ��Ŷ�� �Լ� ���̵�
    // packet : ���� ��Ŷ�� ����Ʈ �迭
    public override void RpcFunction_ReceivePacket(RpcComponentFunctionId functionId, byte[] packet)
    {
        try
        {
            switch (functionId)
            {
                case RpcComponentFunctionId.ServerPurchaseItem:
                    Server_PurchaseItem_ReceivePacket(packet);
                    break;
                case RpcComponentFunctionId.ServerUseItem:
                    Server_UseItem_ReceivePacket(packet);
                    break;
                default:
                    break;

            }
        }
        catch (System.Exception ex)
        {
            Debug.Log($"{ex}");
        }

        base.RpcFunction_ReceivePacket(functionId, packet);
    }

    // Ŭ���̾�Ʈ���� ���� ��Ŷ�� �Ǽ� ��Ŷ���� Ȯ��
    // functionId : ���� ��Ŷ�� �Լ� ���̵�
    // packet : ���� ��Ŷ�� ����Ʈ �迭
    // return : ���� ��Ŷ�� �Ǽ� ��Ŷ�� �ƴ���
    public override bool RpcFunction_Validate(RpcComponentFunctionId functionId, byte[] packet)
    {
        try
        {
            switch (functionId)
            {
                case RpcComponentFunctionId.ServerPurchaseItem:
                    return Server_PurchaseItem_Validate(packet);
                case RpcComponentFunctionId.ServerUseItem:
                    return Server_UseItem_Validate(packet);
                default:
                    break;
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log($"{ex}");
        }

        return base.RpcFunction_Validate(functionId, packet);
    }
    #endregion
}
