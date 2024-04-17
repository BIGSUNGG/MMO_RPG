using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if !UNITY_SERVER
class ClientPacketHandler
{
    public static void C_LoginHandler(ISession session, IMessage packet) { C_Login newPacket = packet as C_Login; }

    public static void C_PongHandler(ISession session, IMessage packet) { C_Pong newPacket = packet as C_Pong; }

    public static void C_ObjectSyncHandler(ISession session, IMessage packet) { C_ObjectSync newPacket = packet as C_ObjectSync; }

   
}
#endif


