using Google.Protobuf;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerPacketMessage
{
	public ushort Id { get; set; }
	public IMessage Message { get; set; }
}

public class ServerPacketQueue
{
	public static ServerPacketQueue Instance { get; } = new ServerPacketQueue();

	Queue<ServerPacketMessage> _packetQueue = new Queue<ServerPacketMessage>();
	object _lock = new object();

	public void Push(ushort id, IMessage packet)
	{
		lock (_lock)
		{
			_packetQueue.Enqueue(new ServerPacketMessage() { Id = id, Message = packet });
		}
	}

	public ServerPacketMessage Pop()
	{
		lock (_lock)
		{
			if (_packetQueue.Count == 0)
				return null;

			return _packetQueue.Dequeue();
		}
	}

	public List<ServerPacketMessage> PopAll()
	{
		List<ServerPacketMessage> list = new List<ServerPacketMessage>();

		lock (_lock)
		{
			while (_packetQueue.Count > 0)
				list.Add(_packetQueue.Dequeue());
		}

		return list;
	}
}