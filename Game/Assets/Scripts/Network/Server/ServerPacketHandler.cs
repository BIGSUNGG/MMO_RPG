using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_SERVER
class ServerPacketHandler
{
	public static void S_ConnectedHandler(ISession session, IMessage packet)
	{
        S_Connected recvPacket = packet as S_Connected;
	}
	public static void S_PingHandler(ISession session, IMessage packet)
	{
        S_Ping recvPacket = packet as S_Ping;

		C_Pong pongPacket = new C_Pong();
		Debug.Log("[Server] PingCheck");
        session.Send(pongPacket);
	}

    public static void S_LoginHandler(ISession session, IMessage packet)
    {
        S_Login recvPacket = packet as S_Login;
        Debug.Log(recvPacket.LoginResult);
    }

    public static void S_EnterMapHandler(ISession session, IMessage packet)
    {
        S_EnterMap recvPacket = packet as S_EnterMap;
        Managers.Map.LoadMap(recvPacket.MapId);
    }
    
    public static void S_EnterPlayerHandler(ISession session, IMessage packet)
    {
        S_EnterPlayer recvPacket = packet as S_EnterPlayer;

        Managers.Network.CreateClienSession(recvPacket.SessionId, recvPacket.AccountDbId);
    }

    public static void S_LeaveMapHandler(ISession session, IMessage packet)
    {
        S_LeaveMap recvPacket = packet as S_LeaveMap;

        Managers.Map.DestroyMap();
    }

    public static void S_LeavePlayerHandler(ISession session, IMessage packet)
    {
        S_LeavePlayer recvPacket = packet as S_LeavePlayer;

        Managers.Network.DeleteClientSession(recvPacket.SessionId);
    }
}
#endif
