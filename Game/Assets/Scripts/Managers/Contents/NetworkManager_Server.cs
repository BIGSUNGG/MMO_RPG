using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using Google.Protobuf;

#if UNITY_SERVER
public partial class NetworkManager
{
	public int GameSessionId;


    public NetworkManager()
	{
    }

    public virtual void Update()
    {
    }

    #region ServerSession
    ServerSession _serverSession = new ServerSession();

    // info : Information of server to connect
    public void ConnectToServer()
    {
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[1];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7778);

        Connector connector = new Connector();

        // Start server connect
        connector.Connect(endPoint,
            () => { return _serverSession; },
            1);
    }

    // packet : Packet to send server
    public void Send(IMessage packet)
    {
        // Send packet to server
        _serverSession.Send(packet);
    }
    #endregion

    #region ClientSession
    Dictionary<int, ClientSession> _clientSessions = new Dictionary<int, ClientSession>();

    // sessionId : 만들 세션의 아이디
    // return : 만든 세션 반환 (실패했을경우 null 반환)
    public ClientSession CreateClienSession(int sessionId)
    {
        if(_clientSessions.ContainsKey(sessionId)) // Is exist same client session id
        {
            Debug.Log("Try create ClientSession but session id " + sessionId + "is already exist");
            return null;
        }

        ClientSession session = new ClientSession(sessionId);
        _clientSessions.Add(sessionId, session);
        return session;
    }

    // sessionId : 제거할 세션의 아이디
    // return : 세션 제거에 성공했는지
    public bool DeleteClientSession(int sessionId)
    {
        // Try remove session
        if (_clientSessions.Remove(sessionId)) 
            return true;

        Debug.Log("Try delete ClientSession but session id " + sessionId + "is not exist");
        return false;
    }

    // sessionId : 찾을 세션의 아이디
    // return : 찾은 세션반환 (실패했을경우 null 반환)
    public ClientSession FindClientSession(int sessionId)
    {
        ClientSession result;
        if(_clientSessions.TryGetValue(sessionId, out result) == false) // Is failed to find session
        {
            Debug.Log("Try find ClientSession but session id " + sessionId + "is not exist");
            return null;
        }

        return result;
    }
    #endregion
}
#endif
