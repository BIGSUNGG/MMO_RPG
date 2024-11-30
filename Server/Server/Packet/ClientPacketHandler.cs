using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using Server.DB;
using Server.Game;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

partial class ClientPacketHandler
{
    public static void C_LoginHandler(ISession session, IMessage packet)
    {
        C_Login recvPacket = packet as C_Login;

        ClientSession clientSession = session as ClientSession;
        if (clientSession == null)
            return;

        clientSession.LoginAccount(recvPacket);
    }

    public static void C_PongHandler(ISession session, IMessage packet)
    {
        ClientSession clientSession = session as ClientSession;
        clientSession.HandlePong();
    }

    public static void C_ObjectSyncHandler(ISession session, IMessage packet)
    {
        C_ObjectSync recvPacket = packet as C_ObjectSync;
        ClientSession clientSession = session as ClientSession;

        GameRoom room = clientSession.MyRoom;
        room.Push(() => { room.RoomSession.Send(clientSession, packet); });
    }

    public static void C_RpcObjectFunctionHandler(ISession session, IMessage packet)
    {
        S_RpcObjectFunction recvPacket = packet as S_RpcObjectFunction;
        ClientSession clientSession = session as ClientSession;

        GameRoom room = clientSession.MyRoom;
        room.Push(() => { room.RoomSession.Send(clientSession, packet); });
    }

    public static void C_RpcComponentFunctionHandler(ISession session, IMessage packet)
    {
        S_RpcComponentFunction recvPacket = packet as S_RpcComponentFunction;
        ClientSession clientSession = session as ClientSession;

        GameRoom room = clientSession.MyRoom;
        room.Push(() => { room.RoomSession.Send(clientSession, packet); });
    }
}