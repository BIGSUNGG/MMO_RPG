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
        List<ServerPacketMessage> list = ServerPacketQueue.Instance.PopAll();
        foreach (ServerPacketMessage packet in list)
        {
            Action<ISession, IMessage> handler = ServerPacketManager.Instance.GetPacketHandler(packet.Id);
            if (handler != null)
                handler.Invoke(_serverSession, packet.Message);
        }
    }
}
#endif
