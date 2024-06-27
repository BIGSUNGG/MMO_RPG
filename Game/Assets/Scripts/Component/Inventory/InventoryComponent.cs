using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
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
    #region Inventory

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
}
