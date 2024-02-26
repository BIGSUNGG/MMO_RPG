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
        Managers.Network.Send(loginPacket);
    }

	public static void S_LoginHandler(ISession session, IMessage packet)
	{
		S_Login loginPacket = packet as S_Login;

        Debug.Log(loginPacket.LoginState);
	}

	public static void S_PingHandler(ISession session, IMessage packet)
	{
        S_Ping pingPacket = packet as S_Ping;

		C_Pong pongPacket = new C_Pong();
		Managers.Network.Send(pongPacket);
	}

    public static void S_EnterMapHandler(ISession session, IMessage packet)
    {
        S_EnterMap mapPacket = packet as S_EnterMap;

    }
}
#endif
