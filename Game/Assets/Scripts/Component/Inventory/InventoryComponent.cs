using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

public class InventoryComponent: ObjectComponent
{

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
    public int _money { get; protected set; } = 0;

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

    #region Purchase
    public void PurchaseItem(NpcController dealer, int itemIndex)
    {
        Debug.Log("H");
        if (dealer._products.Count <= itemIndex)
        {
            Debug.Log("Select wrong index");
            return;
        }

        ProductInfo info = dealer._products[itemIndex];
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
            if (npc._products.Count <= index)
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
                Debug.Log("22H");
        if (dealer._products.Count <= itemIndex)
        {
            Debug.LogError("Select wrong index");
            return;
        }

        ProductInfo info = dealer._products[itemIndex];
        if (info.price > _money) // 돈이 부족한 경우
        {
            Debug.LogError("Need more money");
            return;
        }

        DecreaseMoney(info.price);
    }
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
