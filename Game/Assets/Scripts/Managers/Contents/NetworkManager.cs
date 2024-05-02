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
    public bool IsServer // 현재 프로그램이 서버인지
    {     
        get 
        {
            #if UNITY_SERVER // 서버일 경우
            return true;
            #else
            return false; // 클라이언트인 경우
            #endif
        } 
    }

    public bool IsClient 
    { 
        get 
        { 
            return !IsServer; 
        } 
    }

}
