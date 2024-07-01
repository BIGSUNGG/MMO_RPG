using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        ClientSession clientSession = Managers.Network.CreateClienSession(recvPacket.SessionId);

        // 현재 맵에 있는 모든 오브젝트 클라이언트에게 전송
        {
            S_SpawnObjects objectSpawnInfosPacket = new S_SpawnObjects();
            foreach (var tuple in Managers.Object._objects)
            {
                GameObject go = tuple.Value;
                if (go == null)
                    continue;

                ObjectController oc = go.GetComponent<ObjectController>();
                if (oc == null)
                    continue;

                ObjectInfo info = new ObjectInfo();
                {
                    info.ObjectId = oc.ObjectId;
                    info.ObjectType = oc.ObjectType;
                }
                objectSpawnInfosPacket.SpawnInfos.Add(info);

            }
            Managers.Network.SendClient(clientSession, objectSpawnInfosPacket);
        }

        // 접속한 플레이어가 빙의할 오브젝트 만들기
        {
            ObjectInfo info = new ObjectInfo();
            info.ObjectType = GameObjectType.KnightPlayer;
            GameObject go = Managers.Object.Create(info);

            PlayerController pc = go.GetComponent<PlayerController>();
            if (pc == null)
                return;

            // 만든 오브젝트에 플레이어 빙의시키기
            clientSession.Possess(pc);

            // 체력 설정
            HealthComponent health = go.GetComponent<HealthComponent>();
            health.SetHp(recvPacket.Info.Hp);

            // 돈 설정
            InventoryComponent inventory = go.GetComponent<InventoryComponent>();
            inventory.SetMoney(recvPacket.Info.Money);
        }
    }

    public static void S_LeaveMapHandler(ISession session, IMessage packet)
    {
        S_LeaveMap recvPacket = packet as S_LeaveMap;

        Managers.Map.DestroyMap();
    }

    public static void S_LeavePlayerHandler(ISession session, IMessage packet)
    {
        S_LeavePlayer recvPacket = packet as S_LeavePlayer;

        // 룸에서 나간 플레이어의 클라이언트 세션 찾기
        ClientSession clientSession = Managers.Network.FindClientSession(recvPacket.SessionId);
        if (clientSession == null)
            return;

        // 나간 클라이언트 세션의 오브젝트 제거
        Managers.Object.Delete(clientSession._playerController.ObjectId);
        // 룸에 있는 클라이언트 세션에서 나간 세션 제거
        Managers.Network.DeleteClientSession(recvPacket.SessionId);
    }


    public static void S_SpawnObjectHandler(ISession session, IMessage packet)
    {
        S_SpawnObject recvPacket = packet as S_SpawnObject;

        Managers.Object.Create(recvPacket.SpawnInfo);
    }

    public static void S_SpawnObjectsHandler(ISession session, IMessage packet)
    {
        S_SpawnObjects recvPacket = packet as S_SpawnObjects;

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

    }

    public static void S_UnpossessObjectHandler(ISession session, IMessage packet)
    {
        S_UnpossessObject recvPacket = packet as S_UnpossessObject;

    }

    public static void S_ObjectSyncHandler(ISession session, IMessage packet)
    {
        S_ObjectSync recvPacket = packet as S_ObjectSync;

    }

    public static void S_RequestObjectSyncHandler(ISession session, IMessage packet)
    {
        S_RequestObjectSync recvPacket = packet as S_RequestObjectSync;

    }

    public static void S_RpcObjectFunctionHandler(ISession session, IMessage packet)
    {
        S_RpcObjectFunction recvPacket = packet as S_RpcObjectFunction;
    }

    public static void S_RpcComponentFunctionHandler(ISession session, IMessage packet)
    {
        S_RpcComponentFunction recvPacket = packet as S_RpcComponentFunction;

    }

    public static void S_RequestPlayerInfoHandler(ISession session, IMessage packet)
    {
        S_RequestPlayerInfo recvPacket = packet as S_RequestPlayerInfo;

        // 정보 요청받은 클라이언트 세션 찾기
        ClientSession clientSession = Managers.Network.FindClientSession(recvPacket.SessionId);
        if (clientSession == null)
            return;

        // 정보 보내기
        G_ResponsePlayerInfo sendPacket = new G_ResponsePlayerInfo();
        sendPacket.GameAccountId = recvPacket.GameAccountId;
        sendPacket.Info = clientSession.GetPlayerInfo();
        Managers.Network.SendServer(sendPacket);
    }

    public static void S_NotifyPlayerMoneyHandler(ISession session, IMessage packet)
    {
        S_NotifyPlayerMoney recvPacket = packet as S_NotifyPlayerMoney;
    }

    public static void S_NotifyPlayerItemHandler(ISession session, IMessage packet)
    {
        S_NotifyPlayerItem recvPacket = packet as S_NotifyPlayerItem;
    }
}
#endif
