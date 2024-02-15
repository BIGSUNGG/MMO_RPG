using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if !UNITY_SERVER
class ServerPacketHandler
{
	public static void S_SpawnHandler(ISession session, IMessage packet)
	{
		S_Spawn spawnPacket = packet as S_Spawn;
	}

	public static void S_DespawnHandler(ISession session, IMessage packet)
	{
		S_Despawn despawnPacket = packet as S_Despawn;
	}

    public static void S_ConnectedHandler(ISession session, IMessage packet)
    {
        C_Login loginPacket = new C_Login();

        string path = Application.dataPath;
        loginPacket.UniqueId = path.GetHashCode().ToString();
        Managers.Network.Send(loginPacket);
    }

	public static void S_LoginHandler(ISession session, IMessage packet)
	{
		S_Login loginPacket = packet as S_Login;

	}

	public static void S_PingHandler(ISession session, IMessage packet)
	{
		C_Pong pongPacket = new C_Pong();
		Managers.Network.Send(pongPacket);
	}

    public static void S_EnterMapHandler(ISession session, IMessage packet)
    {
        
    }
}
#endif
