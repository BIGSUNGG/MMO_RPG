﻿using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

#if UNITY_SERVER
public class ServerSession : PacketSession
{
	public void Send(IMessage packet)
	{
		string msgName = packet.Descriptor.Name.Replace("_", string.Empty);
		MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), msgName);
		ushort size = (ushort)packet.CalculateSize();
		byte[] sendBuffer = new byte[size + 4];
		Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
		Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));
		Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);
		Send(new ArraySegment<byte>(sendBuffer));
	}

	public override void OnConnected(EndPoint endPoint)
	{
		Debug.Log($"OnConnected : {endPoint} ");

        ClientPacketManager.Instance.CustomHandler = (s, m, i) =>
		{
			PacketQueue.Instance.Push(i, m);
		};
	}

	public override void OnDisconnected(EndPoint endPoint)
	{
		Debug.Log($"OnDisconnected : {endPoint}");
	}

	public override void OnRecvPacket(ArraySegment<byte> buffer)
	{
        int SessionId = BitConverter.ToInt32(buffer.Array, buffer.Offset);

        ClientSession session = Managers.Network.FindClientSession(SessionId);

        if (session != null)
            ClientPacketManager.Instance.OnRecvPacket(this, new ArraySegment<byte>(buffer.Array, buffer.Offset + 4, buffer.Count - 4));
        else
            Debug.Log("Recv null session's id");
    }

	public override void OnSend(int numOfBytes)
	{
		//Console.WriteLine($"Transferred bytes: {numOfBytes}");
	}
}
#endif