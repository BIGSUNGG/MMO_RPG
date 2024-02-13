using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

#if UNITY_SERVER
public class ClientSession : PacketSession
{
    public ClientSession(int id)
    {
        SessionId = id;
    }

    public int SessionId { get; private set; }

    public override void OnConnected(EndPoint endPoint)
    {
        throw new NotImplementedException();
    }

    public override void OnDisconnected(EndPoint endPoint)
    {
        throw new NotImplementedException();
    }

    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        throw new NotImplementedException();
    }

    public override void OnSend(int numOfBytes)
    {
        throw new NotImplementedException();
    }
}
#endif