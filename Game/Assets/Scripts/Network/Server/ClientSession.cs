using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

#if UNITY_SERVER
public class ClientSession : ISession
{
    public ClientSession(int sessionId)
    {
        SessionId = sessionId;
    }

    public int SessionId { get; private set; }

    public void Send(IMessage packet)
    {
        Managers.Network.SendClient(this, packet);
    }

    #region Controller
    public PlayerController ClientCharacter { get; private set; }

    public void Possess(PlayerController pc, PlayerInfo info)
    {
        if (pc == null)
            return;

        ClientCharacter = pc;
        ClientCharacter.Session = this;

        // 클라이언트에 컨트롤러 빙의 알리기
        S_PossessObject possessPacket = new S_PossessObject();
        possessPacket.ObjectId = pc.ObjectId;
        Managers.Network.SendClient(this, possessPacket);

        // 플레이어 정보 동기화
        SetPlayerInfo(info);
    }

    public PlayerInfo GetPlayerInfo()
    {
        if (ClientCharacter == null)
            return null;

        PlayerInfo result = new PlayerInfo();
        result.SessionId = this.SessionId;
        result.Hp = ClientCharacter.Health.Hp;
        result.Money = ClientCharacter.Inventory.Money;

        foreach (var info in ClientCharacter.Inventory.ItemSlot)
            result.ItemSlot.Add(info);

        return result;
    }

    public void SetPlayerInfo(PlayerInfo info)
    {
        if (ClientCharacter == null)
            return;

        // 체력 설정
        HealthComponent health = ClientCharacter.GetComponent<HealthComponent>();
        health.SetHp(info.Hp);

        // 돈 설정
        InventoryComponent inventory = ClientCharacter.GetComponent<InventoryComponent>();
        inventory.SetMoney(info.Money);
        inventory.SetItemSlot(info.ItemSlot.ToList());
    }
    #endregion

    #region Map
    public bool IsInMap { get; private set; } = true;

    public void MoveMap(int moveMapId)
    {
        if(IsInMap == false)
            return;

        IsInMap = false;

        G_MoveMap sendPacket = new G_MoveMap();
        sendPacket.MoveMapId = moveMapId;
        sendPacket.Info = this.GetPlayerInfo();
        Managers.Network.SendServer(sendPacket);

        Debug.Log($"Move {SessionId} Player to {moveMapId} map");
    }
    #endregion
}
#endif