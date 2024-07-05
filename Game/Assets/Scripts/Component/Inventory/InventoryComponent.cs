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
        if (Util.CheckFuncCalledOnServer() == false) // 서버에서 호출되지않은 경우
            return;

        // 공격한 플레이어의 돈 추가
        PlayerController pc = Owner as PlayerController;
        if (pc == null || pc.Session == null)
            return;

        // 피해자의 HealthComponent 구하기
        HealthComponent victimHealth = victim.GetComponent<HealthComponent>();

        // 처치한 유저의 돈 추가
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

    // 서버와 클라이언트에게 플레이어의 돈 알리기
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
    public const int ItemSlotSize = 9; // 아이템 슬롯 최대 크기
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

    // 아이템 타입에 맞는 슬롯에 아이템 추가
    // itemType : 추가할 아이템의 타입
    // return : 아이템 추가에 성공했는지
    public bool OnServer_AddItem(ItemType itemType)
    {
        if (Util.CheckFuncCalledOnServer() == false)
            return false;

        // 아이템 슬롯에 아이템이 이미 있다면
        for (int i = 0; i < ItemSlotSize; i++)
        {
            if (_itemSlot[i] != null && _itemSlot[i].Type == itemType)
            {
                // 아이탬 개수 추가
                _itemSlot[i].Count++;
                Debug.Log($"Add {itemType}");

                // 서버에게 현재 슬롯 변화 알리기
                Notify_ItemSlot(i);

                return true;
            }
        }

        // 아이템 슬롯에 아이템이 없다면
        for (int i = 0; i < ItemSlotSize; i++)
        {
            if (_itemSlot[i] != null && _itemSlot[i].Type != ItemType.None)
                continue;

            // 빈 공간에 아이템 추가
            _itemSlot[i] = new ItemInfo();
            _itemSlot[i].Type = itemType;
            _itemSlot[i].Count = 1;
            Debug.Log($"Add {itemType}");

            // 서버에게 현재 슬롯 변화 알리기
            Notify_ItemSlot(i);

            return true;
        }

        return false;
    }

    // 아이템 타입에 맞는 슬롯에 아이템 제거
    // index : 아이템을 제거할 슬롯
    // itemType : 제거할 아이템의 타입
    // return : 아이템 제거에 성공했는지
    public bool OnServer_RemoveItem(int index, ItemType itemType)
    {
        if (Util.CheckFuncCalledOnServer() == false)
            return false;

        if (_itemSlot[index] == null || _itemSlot[index].Type == ItemType.None) // 슬롯에 아이템이 없다면
            return false;

        if (_itemSlot[index].Type == itemType) // 슬롯에 있는 아이템이 제거하려는 아이템인지
        {
            // 아이탬 개수 감소
            _itemSlot[index].Count--;
            Debug.Log($"Remove {itemType}");

            if (_itemSlot[index].Count <= 0) // 아이템 개수가 0이하라면
            {
                // 아이템 슬롯에서 아이템 제거
                _itemSlot[index].Type = ItemType.None;
                _itemSlot[index].Count = 0;
            }

            // 서버에게 현재 슬롯 변화 알리기
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

        // 서버에게 현재 슬롯 변화 알리기
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

        // 서버에게 현재 슬롯 변화 알리기
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
        if (ItemSlotSize <= index || _itemSlot[index] == null || _itemSlot[index].Type == ItemType.None) // 아이템 슬롯에 사용할 수 있는 아이템이 없다면
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
        // 패킷 보내기
        if (Managers.Network.IsClient) // 클라이언트에서 호출된 경우 
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
        else // 서버에서 호출된 경우
        {
            Debug.LogError("Client can't call this function");
            return;
        }
    }

    // Rpc 패킷을 받으면 호출
    // packet : 매개변수의 바이트 배열 
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

    // 서버에서 패킷을 받았을 때 악성 패킷을 감지하기 위한 인증
    // packet : 받은 패킷의 바이트 배열
    // return : 받은 패킷이 악성 패킷이 아닌지
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

    // UseItem 코드
    protected virtual void Server_UseItem_Implementation(int index, ItemType itemType)
    {
        // 사용할 아이템 차감
        bool bSuccessRemove = OnServer_RemoveItem(index, itemType);
        if(bSuccessRemove) // 아이템 차감에 성공했다면 
        {
            // 아이템 사용
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
        // 패킷 보내기
        if (Managers.Network.IsClient) // 클라이언트에서 호출된 경우 
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
        else // 서버에서 호출된 경우
        {
            Debug.LogError("Client can't call this function");
            return;
        }
    }

    // Rpc 패킷을 받으면 호출
    // packet : 매개변수의 바이트 배열 
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

    // 서버에서 패킷을 받았을 때 악성 패킷을 감지하기 위한 인증
    // packet : 받은 패킷의 바이트 배열
    // return : 받은 패킷이 악성 패킷이 아닌지
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

    // Server_PurchaseItem 코드
    protected virtual void Server_PurchaseItem_Implementation(NpcController dealer, int itemIndex)
    {
        if (dealer.Products.Count <= itemIndex)
        {
            Debug.LogError("Select wrong index");
            return;
        }

        ProductInfo info = dealer.Products[itemIndex];
        if (info.price > _money) // 돈이 부족한 경우
        {
            Debug.LogError("Need more money");
            return;
        }

        // 돈 차감 이후 아이템 추가
        DecreaseMoney(info.price);
        OnServer_AddItem(info.itemType);
    }
    #endregion

    #endregion

    #region RpcFunction
    // 다른 클라이언트로 패킷을 받으면 FunctionId에 맞는 함수 호출
    // functionId : 받은 패킷의 함수 아이디
    // packet : 받은 패킷의 바이트 배열
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

    // 클라이언트에서 받은 패킷이 악성 패킷인지 확인
    // functionId : 받은 패킷의 함수 아이디
    // packet : 받은 패킷의 바이트 배열
    // return : 받은 패킷이 악성 패킷이 아닌지
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
