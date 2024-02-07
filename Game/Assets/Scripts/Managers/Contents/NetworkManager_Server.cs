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

    // sessionId : Created Session's Id
    // return : Created session (If failed to create return null)
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

    // sessionId : Deleted Session's Id
    // return : Is success to delete session
    public bool DeleteClientSession(int sessionId)
    {
        // Try remove session
        if (_clientSessions.Remove(sessionId)) 
            return true;

        Debug.Log("Try delete ClientSession but session id " + sessionId + "is not exist");
        return false;
    }

    // sessionId : Find Session's Id
    // return : Found session (If failed to find session return null)
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
