using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if !UNITY_SERVER
class ClientPacketHandler
{
    public static void C_LoginHandler(ISession session, IMessage packet) { C_Login recvPacket = packet as C_Login; }

    public static void C_PongHandler(ISession session, IMessage packet) { C_Pong recvPacket = packet as C_Pong; }

    public static void C_RequestObjectInfoHandler(ISession session, IMessage packet) { C_RequestObjectInfo recvPacket = packet as C_RequestObjectInfo; }

    public static void C_ResponseObjectSyncHandler(ISession session, IMessage packet) { C_ResponseObjectSync recvPacket = packet as C_ResponseObjectSync; }

    public static void C_RpcObjectFunctionHandler(ISession session, IMessage packet) { C_RpcObjectFunction recvPacket = packet as C_RpcObjectFunction; }
   
    public static void C_RpcComponentFunctionHandler(ISession session, IMessage packet) { C_RpcComponentFunction recvPacket = packet as C_RpcComponentFunction; }

}
#endif


