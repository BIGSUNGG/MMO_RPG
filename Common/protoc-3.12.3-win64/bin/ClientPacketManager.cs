using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;

class ClientPacketManager
{
	#region Singleton
	static ClientPacketManager _instance = new ClientPacketManager();
	public static ClientPacketManager Instance { get { return _instance; } }
	#endregion

	ClientPacketManager()
	{
		Register();
	}

	Dictionary<ushort, Action<ISession, ArraySegment<byte>, ushort>> _onRecv = new Dictionary<ushort, Action<ISession, ArraySegment<byte>, ushort>>();
	Dictionary<ushort, Action<ISession, IMessage>> _handler = new Dictionary<ushort, Action<ISession, IMessage>>();
		
	public Action<ISession, IMessage, ushort> CustomHandler { get; set; }

	public void Register()
	{		
		_onRecv.Add((ushort)MsgId.CPong, MakePacket<C_Pong>);
		_handler.Add((ushort)MsgId.CPong, ClientPacketHandler.C_PongHandler);		
		_onRecv.Add((ushort)MsgId.CLogin, MakePacket<C_Login>);
		_handler.Add((ushort)MsgId.CLogin, ClientPacketHandler.C_LoginHandler);		
		_onRecv.Add((ushort)MsgId.CObjectSync, MakePacket<C_ObjectSync>);
		_handler.Add((ushort)MsgId.CObjectSync, ClientPacketHandler.C_ObjectSyncHandler);		
		_onRecv.Add((ushort)MsgId.CRpcObjectFunction, MakePacket<C_RpcObjectFunction>);
		_handler.Add((ushort)MsgId.CRpcObjectFunction, ClientPacketHandler.C_RpcObjectFunctionHandler);		
		_onRecv.Add((ushort)MsgId.CRpcComponentFunction, MakePacket<C_RpcComponentFunction>);
		_handler.Add((ushort)MsgId.CRpcComponentFunction, ClientPacketHandler.C_RpcComponentFunctionHandler);
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