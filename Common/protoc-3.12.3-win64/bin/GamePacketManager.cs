using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;

class GamePacketManager
{
	#region Singleton
	static GamePacketManager _instance = new GamePacketManager();
	public static GamePacketManager Instance { get { return _instance; } }
	#endregion

	GamePacketManager()
	{
		Register();
	}

	Dictionary<ushort, Action<ISession, ArraySegment<byte>, ushort>> _onRecv = new Dictionary<ushort, Action<ISession, ArraySegment<byte>, ushort>>();
	Dictionary<ushort, Action<ISession, IMessage>> _handler = new Dictionary<ushort, Action<ISession, IMessage>>();
		
	public Action<ISession, IMessage, ushort> CustomHandler { get; set; }

	public void Register()
	{		
		_onRecv.Add((ushort)MsgId.GMoveMap, MakePacket<G_MoveMap>);
		_handler.Add((ushort)MsgId.GMoveMap, GamePacketHandler.G_MoveMapHandler);		
		_onRecv.Add((ushort)MsgId.GResponsePlayerInfo, MakePacket<G_ResponsePlayerInfo>);
		_handler.Add((ushort)MsgId.GResponsePlayerInfo, GamePacketHandler.G_ResponsePlayerInfoHandler);		
		_onRecv.Add((ushort)MsgId.GNotifyPlayerMoney, MakePacket<G_NotifyPlayerMoney>);
		_handler.Add((ushort)MsgId.GNotifyPlayerMoney, GamePacketHandler.G_NotifyPlayerMoneyHandler);		
		_onRecv.Add((ushort)MsgId.GNotifyPlayerItem, MakePacket<G_NotifyPlayerItem>);
		_handler.Add((ushort)MsgId.GNotifyPlayerItem, GamePacketHandler.G_NotifyPlayerItemHandler);
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