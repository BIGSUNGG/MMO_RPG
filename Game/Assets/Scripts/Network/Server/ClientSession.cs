using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
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
    public PlayerController _playerController { get; private set; }
    public PlayerInfo GetPlayerInfo()
    {
        PlayerInfo result = new PlayerInfo();
        result.SessionId = this.SessionId;

        if (_playerController)
        {
            result.Hp = _playerController.Health._curHp;
            result.Money = _playerController.Inventory._money;
        }

        return result;
    }

    public void Possess(PlayerController pc)
    {
        if (pc == null)
            return;

        _playerController = pc;
        _playerController.Session = this;

        // 클라이언트에 컨트롤러 빙의 알리기
        S_PossessObject possessPacket = new S_PossessObject();
        possessPacket.ObjectId = pc.ObjectId;
        Managers.Network.SendClient(this, possessPacket);
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