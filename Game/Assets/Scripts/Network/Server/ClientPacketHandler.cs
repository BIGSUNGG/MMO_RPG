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

        try
        {
            pc.ObjectSync(info.SyncInfo);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"{ex}");
        }
    }

    public static void C_RpcObjectFunctionHandler(ISession session, IMessage packet)
    {
        C_RpcObjectFunction recvPacket = packet as C_RpcObjectFunction;
        ClientSession clientSession = session as ClientSession;

        PlayerController pc = clientSession._playerController;
        if (pc == null || pc.ObjectId != recvPacket.ObjectId)
            return;

        GameObject go = pc.gameObject;
        if (go == null)
            return;

        // Rpc함수를 호출한 컨트롤러 찾기
        ObjectController objectController = go.GetComponent<ObjectController>();

        // 컨트롤러를 못 찾은 경우
        if (objectController == null)
            return;

        // 받은 패킷이 악성 패킷인 경우
        var parameteByteArr = recvPacket.ParameterBytes.ToByteArray();
        if (objectController.RpcFunction_Validate(recvPacket.RpcFunctionId, parameteByteArr) == false)
        {
            Debug.Log("Receive wrong rpcFunc packet");
            return;
        }

        objectController.RpcFunction_ReceivePacket(recvPacket.RpcFunctionId, parameteByteArr);
    }

    public static void C_RpcComponentFunctionHandler(ISession session, IMessage packet)
    {
        C_RpcComponentFunction recvPacket = packet as C_RpcComponentFunction;
        ClientSession clientSession = session as ClientSession;

        PlayerController pc = clientSession._playerController;
        if (pc == null || pc.ObjectId != recvPacket.ObjectId)
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
        if (objectComp.RpcFunction_Validate(recvPacket.RpcFunctionId, parameteByteArr) == false)
        {
            Debug.Log("Receive wrong rpcFunc packet");
            return;
        }

        objectComp.RpcFunction_ReceivePacket(recvPacket.RpcFunctionId, parameteByteArr);
    }

}
#endif


