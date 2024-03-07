using Google.Protobuf;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientPacketMessage
{
    public ISession Session { get; set; }
	public ushort Id { get; set; }
	public IMessage Message { get; set; }
}

public class ClientPacketQueue
{
	public static ClientPacketQueue Instance { get; } = new ClientPacketQueue();

	Queue<ClientPacketMessage> _packetQueue = new Queue<ClientPacketMessage>();
	object _lock = new object();

	public void Push(ISession session, ushort id, IMessage packet)
	{
		lock (_lock)
		{
			_packetQueue.Enqueue(new ClientPacketMessage() { Session = session, Id = id, Message = packet });
		}
	}

	public ClientPacketMessage Pop()
	{
		lock (_lock)
		{
			if (_packetQueue.Count == 0)
				return null;

			return _packetQueue.Dequeue();
		}
	}

	public List<ClientPacketMessage> PopAll()
	{
		List<ClientPacketMessage> list = new List<ClientPacketMessage>();

		lock (_lock)
		{
			while (_packetQueue.Count > 0)
				list.Add(_packetQueue.Dequeue());
		}

		return list;
	}
}