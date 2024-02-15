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
		C_Login loginPacket = packet as C_Login;
	}

	public static void C_PongHandler(ISession session, IMessage packet)
	{
		ClientSession clientSession = (ClientSession)session;
		clientSession.HandlePong();
	}
}
