using Google.Protobuf;
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
    protected override ushort ParseDataSize(ArraySegment<byte> buffer)
    {
        return BitConverter.ToUInt16(buffer.Array, buffer.Offset + 4);
    }

    public override void Send(IMessage packet)
	{
        Managers.Network.SendServer(packet);  
	}

	public override void OnConnected(EndPoint endPoint)
	{
		Debug.Log($"OnConnected : {endPoint} ");

        ClientPacketManager.Instance.CustomHandler = (s, m, i) =>
		{
			ClientPacketQueue.Instance.Push(s, i, m);
		};
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
#if true // Log Packet Info
        Debug.Log(
            "Id : " + BitConverter.ToInt32(buffer.Array, buffer.Offset) +
            ", Size : " + BitConverter.ToUInt16(buffer.Array, buffer.Offset + 4) +
            ", MsgId : " + BitConverter.ToUInt16(buffer.Array, buffer.Offset + 6)
            );
#endif

        int sessionId = BitConverter.ToInt32(buffer.Array, buffer.Offset);
        if(sessionId == 0) // Recieve server packet
        {           
            ServerPacketManager.Instance.OnRecvPacket(this, new ArraySegment<byte>(buffer.Array, buffer.Offset + 4, buffer.Count - 4));
        }
        else // Recieve client packet
        {        
            ClientSession session = Managers.Network.FindClientSession(sessionId);

            if (session != null)
                ClientPacketManager.Instance.OnRecvPacket(session, new ArraySegment<byte>(buffer.Array, buffer.Offset + 4, buffer.Count - 4));
            else
                Debug.Log("Recv null session's id");
        }
    }

	public override void OnSend(int numOfBytes)
	{
		//Console.WriteLine($"Transferred bytes: {numOfBytes}");
	}
}
#endif