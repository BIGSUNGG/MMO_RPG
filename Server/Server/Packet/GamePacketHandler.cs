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

        GameInstanceManager.Instance.Push(() => 
        {
            clientSession.LeaveMap();
            clientSession.EnterMap(moveMap);
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
}