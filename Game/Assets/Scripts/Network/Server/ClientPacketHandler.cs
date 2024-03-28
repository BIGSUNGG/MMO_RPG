using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_SERVER
class ClientPacketHandler
{
    public static void C_LoginHandler(ISession session, IMessage packet) { }

    public static void C_EnterGameHandler(ISession session, IMessage packet) { }

    public static void C_CreatePlayerHandler(ISession session, IMessage packet) { }

    public static void C_PongHandler(ISession session, IMessage packet) { }

    public static void C_ObjectSyncHandler(ISession session, IMessage packet) 
    {
        ClientSession clientSession = session as ClientSession;
        if (clientSession == null)
        {
            Debug.Log("Client session is null");
            return;
        }

        C_ObjectSync recvPacket = packet as C_ObjectSync;
        ObjectSyncInfo info = recvPacket.SyncInfo;

        PlayerController pc = clientSession._playerController;
        if (pc == null)
        {
            Debug.Log("Player controller is null");
            return;
        }

        Debug.Log($"{pc.ObjectId} ObjectSync");
        pc.ObjectSync(info.SyncInfoJson);
    }

}
#endif


