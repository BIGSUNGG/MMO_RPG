using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

#if !UNITY_SERVER
public class ClientSession : ISession
{
    public ClientSession(int accountDbId)
    {
    }

    public int SessionId { get; private set; }

    void ISession.Send(IMessage packet)
    {
        throw new NotImplementedException();
    }

    public void MoveMap(int moveMapId)
    {
        throw new NotImplementedException();
    }
}
#endif