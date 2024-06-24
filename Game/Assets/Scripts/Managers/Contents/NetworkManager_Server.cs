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

    public void Init()
    {
        Managers.Timer.SetTimer(0.0167f, Sync, true);
    }

    public virtual void Update()
    {
        // 서버 패킷 처리
        List<ServerPacketMessage> serverList = ServerPacketQueue.Instance.PopAll();
        foreach (ServerPacketMessage packet in serverList)
        {
            Action<ISession, IMessage> handler = ServerPacketManager.Instance.GetPacketHandler(packet.Id);
            if (handler != null)
                handler.Invoke(_serverSession, packet.Message);
        }

        // 클라이언트 패킷 처리
        List<ClientPacketMessage> clientList = ClientPacketQueue.Instance.PopAll();
        foreach (ClientPacketMessage packet in clientList)
        {
            Action<ISession, IMessage> handler = ClientPacketManager.Instance.GetPacketHandler(packet.Id);
            if (handler != null)
                handler.Invoke(packet.Session, packet.Message);
        }  
    }

    #region Sync
    protected void Sync()
    {
        if (_clientSessions.Count > 0)
        {
            S_RequestObjectSync requestPacket = new S_RequestObjectSync();
            SendMulticast(requestPacket);

            // 오브젝트 정보 클라이언트와 싱크맞추기
            S_ObjectSync syncPacket = new S_ObjectSync();
            foreach (var tuple in Managers.Object._objects)
            {
                GameObject go = tuple.Value;
                if (go == null)
                    continue;

                ObjectController oc = go.GetComponent<ObjectController>();
                if (oc == null)
                    continue;

                ObjectSyncInfo info = new ObjectSyncInfo();
                info.ObjectInfo = new ObjectInfo();

                info.SyncInfo = oc.GetObjectSyncInfo();
                info.ObjectInfo.ObjectId = oc.ObjectId;
                info.ObjectInfo.ObjectType = oc.ObjectType;
                syncPacket.SyncInfos.Add(info);
            }

            SendMulticast(syncPacket);
        }
    }
    #endregion

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
        Array.Copy(BitConverter.GetBytes((ushort)(size + 8)), 0, sendBuffer, 4, sizeof(ushort));
        Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 6, sizeof(ushort));
        Array.Copy(packet.ToByteArray(), 0, sendBuffer, 8, size);

        // 서버로 패킷 전송
        _serverSession.Send(new ArraySegment<byte>(sendBuffer));

#if false // Log Packet Info
        Debug.Log(
            "Send " +
            "Id : " + sessionId +
            ", Size : " + (size + 8) +
            ", MsgId : " + msgId + $"{((int)msgId)}"
            );
#endif
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
        Array.Copy(BitConverter.GetBytes((ushort)(size + 8)), 0, sendBuffer, 4, sizeof(ushort));
        Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 6, sizeof(ushort));
        Array.Copy(packet.ToByteArray(), 0, sendBuffer, 8, size);

        // 서버로 패킷 전송
        _serverSession.Send(new ArraySegment<byte>(sendBuffer));

#if false // Log Packet Info
        Debug.Log(
            "Send " +
            "Id : " + session.SessionId +
            ", Size : " + (size + 8) +
            ", MsgId : " + msgId + $"{((int)msgId)}"
            );
#endif
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
        Array.Copy(BitConverter.GetBytes((ushort)(size + 8)), 0, sendBuffer, 4, sizeof(ushort));
        Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 6, sizeof(ushort));
        Array.Copy(packet.ToByteArray(), 0, sendBuffer, 8, size);

        // 서버로 패킷 전송
        _serverSession.Send(new ArraySegment<byte>(sendBuffer));

#if false // Log Packet Info
        Debug.Log(
            "Send " +
            "Id : " + sessionId +
            ", Size : " + (size + 8) +
            ", MsgId : " + msgId + $"{((int)msgId)}"
            );
#endif
    }

    #endregion

    #region ClientSession
    Dictionary<int, ClientSession> _clientSessions = new Dictionary<int, ClientSession>(); // Key : ClientSession의 세션 아이디, Value : 세션 아이디에 맞는 ClientSession

    // accountId : 만들 세션의 세션 아이디
    // return : 만든 세션 반환 (실패했을경우 null 반환)
    public ClientSession CreateClienSession(int accountDbId)
    {
        if(_clientSessions.ContainsKey(accountDbId)) // Is exist same client session id
        {
            Debug.Log("Try create ClientSession but session id " + accountDbId + "is already exist");
            return null;
        }

        ClientSession session = new ClientSession(accountDbId);
        _clientSessions.Add(accountDbId, session);
        return session;
    }

    // accountId : 제거할 세션의 세션 아이디
    // return : 세션 제거에 성공했는지
    public bool DeleteClientSession(int accountId)
    {
        // Try remove session
        if (_clientSessions.Remove(accountId)) 
            return true;

        Debug.Log("Try delete ClientSession but account id " + accountId + "is not exist");
        return false;
    }

    // accountId : 찾을 세션의 세션 아이디
    // return : 찾은 세션반환 (실패했을경우 null 반환)
    public ClientSession FindClientSession(int accountId)
    {
        ClientSession result;
        if(_clientSessions.TryGetValue(accountId, out result) == false) // Is failed to find session
            return null;

        return result;
    }
    #endregion 
}
#endif
