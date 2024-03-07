using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

#if UNITY_SERVER
public class ClientSession : ISession
{
    public ClientSession(int id)
    {
        SessionId = id;
    }

    public int SessionId { get; private set; }

    public void Send(IMessage packet)
    {
        Managers.Network.SendClient(this, packet);
    }
}
#endif