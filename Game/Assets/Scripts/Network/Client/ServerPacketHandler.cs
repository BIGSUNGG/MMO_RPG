﻿using Google.Protobuf;
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
		S_Connected recvPacket = packet as S_Connected;

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
		S_Login recvPacket = packet as S_Login;

	}

	public static void S_PingHandler(ISession session, IMessage packet)
	{
		S_Ping recvPacket = packet as S_Ping;

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
		S_LeaveMap recvPacket = packet as S_LeaveMap;

	}

	public static void S_LeavePlayerHandler(ISession session, IMessage packet)
	{
		S_LeavePlayer recvPacket = packet as S_LeavePlayer;

	}

	public static void S_SpawnObjectHandler(ISession session, IMessage packet)
	{
		S_SpawnObject recvPacket = packet as S_SpawnObject;

        Debug.Log("H");
		Managers.Object.Create(recvPacket.SpawnInfo);
	}

    public static void S_SpawnObjectsHandler(ISession session, IMessage packet)
    {
        S_SpawnObjects recvPacket = packet as S_SpawnObjects;

        Debug.Log("H1");
        Managers.Object.Create(recvPacket.SpawnInfos);
    }

    public static void S_DespawnObjectHandler(ISession session, IMessage packet)
	{
		S_DespawnObject recvPacket = packet as S_DespawnObject;   
        
		Managers.Object.Delete(recvPacket.ObjectId);
	}

    public static void S_DespawnObjectsHandler(ISession session, IMessage packet)
    {
        S_DespawnObjects recvPacket = packet as S_DespawnObjects;
        Managers.Object.Delete(recvPacket.ObjectIds);
    }

    public static void S_PossessObjectHandler(ISession session, IMessage packet)
	{
		S_PossessObject recvPacket = packet as S_PossessObject;

        Debug.Log("H2");
		GameObject obj = Managers.Object.FindById(recvPacket.ObjectId);
		Managers.Controller.Possess(obj);
	}

	public static void S_UnpossessObjectHandler(ISession session, IMessage packet)
	{
		S_UnpossessObject recvPacket = packet as S_UnpossessObject;

	}

	public static void S_ObjectSyncHandler(ISession session, IMessage packet)
	{
		S_ObjectSync recvPacket = packet as S_ObjectSync;

		foreach (var info in recvPacket.SyncInfos)
		{
			GameObject go = Managers.Object.FindById(info.ObjectInfo.ObjectId);
			if (go == null)
				return;

			ObjectController controller = go.GetComponent<ObjectController>();
			if (controller == null)
				return;

			controller.ObjectSync(info.SyncInfoJson);
		}
	}
}

#endif
