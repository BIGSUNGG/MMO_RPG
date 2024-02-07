using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

#if UNITY_SERVER
public class ClientSession
{
    public ClientSession(int id)
    {
        SessionId = id;
    }

    public int SessionId { get; private set; }
}
#endif