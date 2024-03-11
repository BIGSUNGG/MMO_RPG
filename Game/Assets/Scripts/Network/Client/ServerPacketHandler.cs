using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if !UNITY_SERVER
class ServerPacketHandler
{
    public static void S_ConnectedHandler(ISession session, IMessage packet)
    {
        S_Connected connectedPacket = packet as S_Connected;

        // 로그인 패킷 보내기
        C_Login loginPacket = new C_Login();

#if true // 정상적인 패킷 보내기
        loginPacket.AccountId = Managers.Network.AccountId;
        loginPacket.Token = Managers.Network.Token;
#else // 비정상적인 패킷 보내기
        loginPacket.AccountId = 11231223;
        loginPacket.Token = 129212222;
#endif

        session.Send(loginPacket);
    }

	public static void S_LoginHandler(ISession session, IMessage packet)
	{
		S_Login loginPacket = packet as S_Login;

        Debug.Log(loginPacket.LoginResult);
        if(loginPacket.LoginResult == LoginResult.LoginSuccess)
        {
            Managers.Map.LoadMap(0);
        }
    }

	public static void S_PingHandler(ISession session, IMessage packet)
	{
        S_Ping pingPacket = packet as S_Ping;

		C_Pong pongPacket = new C_Pong();
		Managers.Network.Send(pongPacket);
	}

    public static void S_EnterMapHandler(ISession session, IMessage packet)
    {
        S_EnterMap recvPacket = packet as S_EnterMap;
        Managers.Map.LoadMap(recvPacket.MapId);

    }

    public static void S_EnterPlayerHandler(ISession session, IMessage packet)
    {
        S_EnterPlayer recvPacket = packet as S_EnterPlayer;

        Debug.Log($"Played Id : {recvPacket.AccountDbId} Enter");
    }

    public static void S_LeaveMapHandler(ISession session, IMessage packet)
    {
        S_LeaveMap leaveMapPacket = packet as S_LeaveMap;

    }

    public static void S_LeavePlayerHandler(ISession session, IMessage packet)
    {
        S_LeavePlayer leavePlayerPacket = packet as S_LeavePlayer;

    }
}
#endif
