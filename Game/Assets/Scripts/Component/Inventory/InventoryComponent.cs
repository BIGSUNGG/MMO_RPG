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

    // Server_PurchaseItem �ڵ�
    protected virtual void Server_PurchaseItem_Implementation(NpcController dealer, int itemIndex)
    {
                Debug.Log("22H");
        if (dealer._products.Count <= itemIndex)
        {
            Debug.LogError("Select wrong index");
            return;
        }

        ProductInfo info = dealer._products[itemIndex];
        if (info.price > _money) // ���� ������ ���
        {
            Debug.LogError("Need more money");
            return;
        }

        DecreaseMoney(info.price);
    }
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
