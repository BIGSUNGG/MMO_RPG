#if !UNITY_SERVER
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using Google.Protobuf;
using Google.Protobuf.Protocol;

public partial class NetworkManager
{
    public bool IsServer { get { return false; } private set { } }

    public int AccountId { get; set; }
    public int Token { get; set; }

    ServerSession _serverSession = new ServerSession();

    public void Send(IMessage packet)
    {
        string msgName = packet.Descriptor.Name.Replace("_", string.Empty);
        MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), msgName);
        ushort size = (ushort)packet.CalculateSize();
        byte[] sendBuffer = new byte[size + 4];
        Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
        Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));
        Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);
        _serverSession.Send(new ArraySegment<byte>(sendBuffer));
    }

    public void ConnectToGame(ServerInfo info)
    {
        IPAddress ipAddr = IPAddress.Parse(info.IpAddress);
        IPEndPoint endPoint = new IPEndPoint(ipAddr, info.Port);
        
        Connector connector = new Connector();
        
        connector.Connect(endPoint,
            () => { return _serverSession; },
            1);
    }

    public void Update()
    {
        // 서버 패킷 처리
        List<ServerPacketMessage> list = ServerPacketQueue.Instance.PopAll();
        foreach (ServerPacketMessage packet in list)
        {
            Action<ISession, IMessage> handler = ServerPacketManager.Instance.GetPacketHandler(packet.Id);
            if (handler != null)
                handler.Invoke(_serverSession, packet.Message);
        }

        //// 내 컨트롤러 정보 서버와 싱크맞추기
        //PlayerController pc = Managers.Controller.MyController;
        //if (pc)
        //{
	    //    C_ObjectSync syncPacket = new C_ObjectSync();
	    //    syncPacket.SyncInfo.SyncInfoJson = pc.GetObjectSyncInfo();
        //    syncPacket.SyncInfo.ObjectInfo.ObjectId = pc.ObjectId;
        //    syncPacket.SyncInfo.ObjectInfo.ObjectType = pc.ObjectType;
        //    Send(syncPacket);
        //}
        
    }
}
#endif
