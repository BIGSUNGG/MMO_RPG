using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_SERVER
class ClientPacketHandler
{
    public static void C_LoginHandler(ISession session, IMessage packet) { C_Login recvPacket = packet as C_Login; }

    public static void C_PongHandler(ISession session, IMessage packet) { C_Pong recvPacket = packet as C_Pong; }

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
        else if(pc.ObjectId != recvPacket.SyncInfo.ObjectInfo.ObjectId)
        {
            Debug.Log("Player controller id is not sync info's id");
            return;
        }

        pc.ObjectSync(info.SyncInfo);
    }

    public static void C_DodgeStartHandler(ISession session, IMessage packet) 
    { 
        C_DodgeStart recvPacket = packet as C_DodgeStart;
        ClientSession clientSession = session as ClientSession;

        S_DodgeStart sendPacket = new S_DodgeStart();
        sendPacket.ObjectId = clientSession._playerController.ObjectId;
        sendPacket.X = recvPacket.X;
        sendPacket.Y = recvPacket.Y;
        Managers.Network.SendMulticast(sendPacket);

    }

}
#endif


