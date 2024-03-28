using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

#if !UNITY_SERVER
public class ServerSession : PacketSession
{
	public override void Send(IMessage packet)
	{
        Managers.Network.Send(packet);  
    }

    public override void OnConnected(EndPoint endPoint)
	{
		Debug.Log($"OnConnected : {endPoint}");

		ServerPacketManager.Instance.CustomHandler = (s, m, i) =>
		{
            ServerPacketQueue.Instance.Push(i, m);
		};
	}

	public override void OnDisconnected(EndPoint endPoint)
	{
		Debug.Log($"OnDisconnected : {endPoint}");
	}

	public override void OnRecvPacket(ArraySegment<byte> buffer)
	{
        #if false // Log Packet Info
        Debug.Log(
            "Size : " + BitConverter.ToUInt16(buffer.Array, buffer.Offset ) +
            ", MsgId : " + BitConverter.ToUInt16(buffer.Array, buffer.Offset + 2)
            );
        #endif

        ServerPacketManager.Instance.OnRecvPacket(this, buffer);
	}

	public override void OnSend(int numOfBytes)
	{
		//Console.WriteLine($"Transferred bytes: {numOfBytes}");
	}
}
#endif