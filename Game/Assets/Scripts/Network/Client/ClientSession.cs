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

    void ISession.Send(IMessage packet)
    {
        throw new NotImplementedException();
    }
}
#endif