using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using Google.Protobuf;

public interface INetworkManager
{
    public void Update();
}

public partial class NetworkManager : INetworkManager
{

}
