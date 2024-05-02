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

    public static void C_RpcFunctionHandler(ISession session, IMessage packet)
    {
        C_RpcFunction recvPacket = packet as C_RpcFunction;
        ClientSession clientSession = session as ClientSession;

        PlayerController pc = clientSession._playerController;
        if (pc == null)
            return;

        GameObject go = pc.gameObject;
        if (go == null)
            return;

        // Rpc함수를 호출한 컴포넌트 찾기
        ObjectComponent objectComp = go.GetComponent(recvPacket.ComponentType);

        // 컴포넌트를 못 찾은 경우
        if (objectComp == null)
            return;

        // 받은 패킷이 악성 패킷인 경우
        var parameteByteArr = recvPacket.ParameterBytes.ToByteArray();
        if (objectComp.RpcFunction_Validate(parameteByteArr) == false)
        {
            Debug.Log("Receive wrong rpcFunc packet");
            return;
        }

        objectComp.RpcFunction_ReceivePacket(parameteByteArr);
    }

}
#endif


