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


}
#endif


