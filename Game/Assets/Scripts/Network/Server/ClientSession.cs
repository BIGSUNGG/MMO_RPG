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
    public ClientSession(int accountId)
    {
        AccountDbId = accountId;
    }

    public int AccountDbId { get; private set; }

    public void Send(IMessage packet)
    {
        Managers.Network.SendClient(this, packet);
    }

    #region Controller
    public PlayerController _playerController { get; private set; }

    public void Possess(PlayerController pc)
    {
        if (pc == null)
            return;

        _playerController = pc;

        // 클라이언트에 컨트롤러 빙의 알리기
        S_PossessObject possessPacket = new S_PossessObject();
        possessPacket.ObjectId = pc.ObjectId;
        Managers.Network.SendClient(this, possessPacket);
    }

    #endregion
}
#endif