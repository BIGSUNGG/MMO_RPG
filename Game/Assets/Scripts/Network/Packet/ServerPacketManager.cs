using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;

class ServerPacketManager
{
	#region Singleton
	static ServerPacketManager _instance = new ServerPacketManager();
	public static ServerPacketManager Instance { get { return _instance; } }
	#endregion

	ServerPacketManager()
	{
		Register();
	}

	Dictionary<ushort, Action<ISession, ArraySegment<byte>, ushort>> _onRecv = new Dictionary<ushort, Action<ISession, ArraySegment<byte>, ushort>>();
	Dictionary<ushort, Action<ISession, IMessage>> _handler = new Dictionary<ushort, Action<ISession, IMessage>>();
		
	public Action<ISession, IMessage, ushort> CustomHandler { get; set; }

	public void Register()
	{		
		_onRecv.Add((ushort)MsgId.SConnected, MakePacket<S_Connected>);
		_handler.Add((ushort)MsgId.SConnected, ServerPacketHandler.S_ConnectedHandler);		
		_onRecv.Add((ushort)MsgId.SPing, MakePacket<S_Ping>);
		_handler.Add((ushort)MsgId.SPing, ServerPacketHandler.S_PingHandler);		
		_onRecv.Add((ushort)MsgId.SLogin, MakePacket<S_Login>);
		_handler.Add((ushort)MsgId.SLogin, ServerPacketHandler.S_LoginHandler);		
		_onRecv.Add((ushort)MsgId.SEnterMap, MakePacket<S_EnterMap>);
		_handler.Add((ushort)MsgId.SEnterMap, ServerPacketHandler.S_EnterMapHandler);		
		_onRecv.Add((ushort)MsgId.SEnterPlayer, MakePacket<S_EnterPlayer>);
		_handler.Add((ushort)MsgId.SEnterPlayer, ServerPacketHandler.S_EnterPlayerHandler);		
		_onRecv.Add((ushort)MsgId.SLeaveMap, MakePacket<S_LeaveMap>);
		_handler.Add((ushort)MsgId.SLeaveMap, ServerPacketHandler.S_LeaveMapHandler);		
		_onRecv.Add((ushort)MsgId.SLeavePlayer, MakePacket<S_LeavePlayer>);
		_handler.Add((ushort)MsgId.SLeavePlayer, ServerPacketHandler.S_LeavePlayerHandler);		
		_onRecv.Add((ushort)MsgId.SSpawnObject, MakePacket<S_SpawnObject>);
		_handler.Add((ushort)MsgId.SSpawnObject, ServerPacketHandler.S_SpawnObjectHandler);		
		_onRecv.Add((ushort)MsgId.SDespawnObject, MakePacket<S_DespawnObject>);
		_handler.Add((ushort)MsgId.SDespawnObject, ServerPacketHandler.S_DespawnObjectHandler);		
		_onRecv.Add((ushort)MsgId.SPossessObject, MakePacket<S_PossessObject>);
		_handler.Add((ushort)MsgId.SPossessObject, ServerPacketHandler.S_PossessObjectHandler);		
		_onRecv.Add((ushort)MsgId.SUnpossessObject, MakePacket<S_UnpossessObject>);
		_handler.Add((ushort)MsgId.SUnpossessObject, ServerPacketHandler.S_UnpossessObjectHandler);
	}

	public void OnRecvPacket(ISession session, ArraySegment<byte> buffer)
	{
		ushort count = 0;

		ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
		count += 2;
		ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
		count += 2;

		Action<ISession, ArraySegment<byte>, ushort> action = null;
		if (_onRecv.TryGetValue(id, out action))
			action.Invoke(session, buffer, id);
	}

	void MakePacket<T>(ISession session, ArraySegment<byte> buffer, ushort id) where T : IMessage, new()
	{
		T pkt = new T();
		pkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);

		if (CustomHandler != null)
		{
			CustomHandler.Invoke(session, pkt, id);
		}
		else
		{
			Action<ISession, IMessage> action = null;
			if (_handler.TryGetValue(id, out action))
				action.Invoke(session, pkt);
		}
	}

	public Action<ISession, IMessage> GetPacketHandler(ushort id)
	{
		Action<ISession, IMessage> action = null;
		if (_handler.TryGetValue(id, out action))
			return action;
		return null;
	}
}