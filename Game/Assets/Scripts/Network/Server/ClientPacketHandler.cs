using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_SERVER
partial class ClientPacketHandler
{
    public static void C_LoginHandler(PacketSession session, IMessage packet) { }

    public static void C_EnterGameHandler(PacketSession session, IMessage packet) { }

    public static void C_CreatePlayerHandler(PacketSession session, IMessage packet) { }

    public static void C_PongHandler(PacketSession session, IMessage packet) { }


}
#endif


