using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using static System.Collections.Specialized.BitVector32;

#if UNITY_SERVER
public partial class NetworkManager
{
	public int GameSessionId;


    public NetworkManager()
	{
    }

    public virtual void Update()
    {
        List<ServerPacketMessage> serverList = ServerPacketQueue.Instance.PopAll();
        foreach (ServerPacketMessage packet in serverList)
        {
            Action<ISession, IMessage> handler = ServerPacketManager.Instance.GetPacketHandler(packet.Id);
            if (handler != null)
                handler.Invoke(_serverSession, packet.Message);
        }

        List<ClientPacketMessage> clientList = ClientPacketQueue.Instance.PopAll();
        foreach (ClientPacketMessage packet in clientList)
        {
            Action<ISession, IMessage> handler = ClientPacketManager.Instance.GetPacketHandler(packet.Id);
            if (handler != null)
                handler.Invoke(_serverSession, packet.Message);
        }
    }

    #region ServerSession
    ServerSession _serverSession = new ServerSession();

    // info : 연결할 서버의 정보
    public void ConnectToServer()
    {
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[1];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7778);

        Connector connector = new Connector();

        // 서버에 연결 시도
        connector.Connect(endPoint,
            () => { return _serverSession; },
            1);
    }

    // packet : 서버에 전송할 패킷
    public void SendServer(IMessage packet)
    {
        // 서버에게 sessionId로 0을 준다면 서버에서 처리하는 패킷으로 처리
        int sessionId = 0;

		string msgName = packet.Descriptor.Name.Replace("_", string.Empty);
		MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), msgName);
		ushort size = (ushort)packet.CalculateSize();
		byte[] sendBuffer = new byte[size + 8];
		Array.Copy(BitConverter.GetBytes((int)(sessionId)), 0, sendBuffer, 0, sizeof(int));
		Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 4, sizeof(ushort));
		Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 6, sizeof(ushort));
		Array.Copy(packet.ToByteArray(), 0, sendBuffer, 8, size);

        // 서버로 패킷 전송
        _serverSession.Send(new ArraySegment<byte>(sendBuffer));
    }

    // 매개 변수로 들어온 session으로 패킷 전송
    public void SendClient(ClientSession session, IMessage packet)
    {
        // 서버로 패킷 전송 후 서버에서 다시 클라이언트로 전송
		string msgName = packet.Descriptor.Name.Replace("_", string.Empty);
		MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), msgName);
		ushort size = (ushort)packet.CalculateSize();
		byte[] sendBuffer = new byte[size + 8];
		Array.Copy(BitConverter.GetBytes((int)(session.SessionId)), 0, sendBuffer, 0, sizeof(int));
		Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 4, sizeof(ushort));
		Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 6, sizeof(ushort));
		Array.Copy(packet.ToByteArray(), 0, sendBuffer, 8, size);

        // 서버로 패킷 전송
        _serverSession.Send(new ArraySegment<byte>(sendBuffer));
    }

    // 이 Game Room안에 있는 모든 클라이언트에게 패킷 전송
    public void SendMulticast(IMessage packet)
    {
        // 서버에게 sessionId로 -1을 준다면 모두에게 전송하는 패킷으로 처리
        int sessionId = -1;

        // 서버로 패킷 전송 후 서버에서 모든 클라이언트로 전송
        string msgName = packet.Descriptor.Name.Replace("_", string.Empty);
        MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), msgName);
        ushort size = (ushort)packet.CalculateSize();
        byte[] sendBuffer = new byte[size + 8];
        Array.Copy(BitConverter.GetBytes((int)(sessionId)), 0, sendBuffer, 0, sizeof(int));
        Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 4, sizeof(ushort));
        Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 6, sizeof(ushort));
        Array.Copy(packet.ToByteArray(), 0, sendBuffer, 8, size);

        // 서버로 패킷 전송
        _serverSession.Send(new ArraySegment<byte>(sendBuffer));
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
