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
        S_Connected connectedPacket = packet as S_Connected;
	}
	public static void S_PingHandler(ISession session, IMessage packet)
	{
		C_Pong pongPacket = new C_Pong();
		Debug.Log("[Server] PingCheck");
		Managers.Network.Send(pongPacket);
	}

    public static void S_LoginHandler(ISession session, IMessage packet)
    {
        S_Login loginPacket = packet as S_Login;
        Debug.Log(loginPacket.LoginState);
    }

    public static void S_EnterMapHandler(ISession session, IMessage packet)
    {
        S_EnterMap roomPacket = (S_EnterMap)packet;
        Managers.Map.LoadMap(roomPacket.MapId);
    }
}
#endif
