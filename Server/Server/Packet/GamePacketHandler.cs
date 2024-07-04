using Google.Protobuf;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using Server;
using Server.DB;
using Server.Game;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

partial class GamePacketHandler
{
    public static void G_MoveMapHandler(ISession session, IMessage packet)
    {
        G_MoveMap recvPacket = packet as G_MoveMap;
        GameSession gameSession = session as GameSession;
        GameInstance curMap = gameSession.Map;

        ClientSession clientSession = curMap.FindSession(recvPacket.Info.SessionId);
        if (clientSession == null)
            return;

        GameInstance moveMap = GameInstanceManager.Instance.Find(recvPacket.MoveMapId);
        if (moveMap == null)
            return;

        clientSession.SetPlayerInfo(recvPacket.Info);
        DbTransaction.SavePlayer(clientSession.GameAccountDbId, curMap.MapId, clientSession.GetPlayerInfo());

        GameInstanceManager.Instance.Push(() => 
        {
            clientSession.LeaveMap();
            clientSession.EnterMap(moveMap, 1250);
        });        
    }

    public static void G_ResponsePlayerInfoHandler(ISession session, IMessage packet)
    {
        G_ResponsePlayerInfo recvPacket = packet as G_ResponsePlayerInfo;
        GameSession gameSession = session as GameSession;
        GameInstance curMap = gameSession.Map;

        DbTransaction.SavePlayer(recvPacket.GameAccountId, curMap.MapId, recvPacket.Info);
    }

    public static void G_NotifyPlayerMoneyHandler(ISession session, IMessage packet)
    {
        G_NotifyPlayerMoney recvPacket = packet as G_NotifyPlayerMoney;
        GameSession gameSession = session as GameSession;
        GameInstance curMap = gameSession.Map;

        ClientSession clientSession = curMap.FindSession(recvPacket.SessionId);
        if (clientSession == null)
            return;

        clientSession.Money = recvPacket.Money;

        // 클라이언트에게 돈 전송
        S_NotifyPlayerMoney sendPacket = new S_NotifyPlayerMoney();
        sendPacket.Money = recvPacket.Money;

        clientSession.Send(sendPacket);
    }

    public static void G_NotifyPlayerItemSlotHandler(ISession session, IMessage packet)
    {
        G_NotifyPlayerItemSlot recvPacket = packet as G_NotifyPlayerItemSlot;
        GameSession gameSession = session as GameSession;
        GameInstance curMap = gameSession.Map;

        ClientSession clientSession = curMap.FindSession(recvPacket.SessionId);
        if (clientSession == null)
            return;

        clientSession.ItemSlot[recvPacket.Index] = recvPacket.Info;

        // 클라이언트에게 돈 전송
        S_NotifyPlayerItemSlot sendPacket = new S_NotifyPlayerItemSlot();
        sendPacket.Index = recvPacket.Index;
        sendPacket.Info = recvPacket.Info;

        clientSession.Send(sendPacket);
    }

    public static void G_NotifyPlayerItemSlotAllHandler(ISession session, IMessage packet)
    {
        G_NotifyPlayerItemSlotAll recvPacket = packet as G_NotifyPlayerItemSlotAll;
        GameSession gameSession = session as GameSession;
        GameInstance curMap = gameSession.Map;

        ClientSession clientSession = curMap.FindSession(recvPacket.SessionId);
        if (clientSession == null)
            return;

        if (recvPacket.ItemSlot == null)
            return;

        clientSession.SetItemSlot(recvPacket.ItemSlot.ToList());

        // 클라이언트에게 아이템 슬롯 정보 전송
        S_NotifyPlayerItemSlotAll sendPacket = new S_NotifyPlayerItemSlotAll();
        foreach (var info in recvPacket.ItemSlot)
            sendPacket.ItemSlot.Add(info);

        clientSession.Send(sendPacket);
    }
}
