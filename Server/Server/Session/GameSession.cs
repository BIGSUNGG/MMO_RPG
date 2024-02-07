using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;
using System.Net;
using Google.Protobuf.Protocol;
using Google.Protobuf;
using Server.Game;
using Server.Data;
using System.Diagnostics;

namespace Server
{
    public partial class GameSession : PacketSession
    {
        public GameRoom Room { get; set; }

		object _lock = new object();
		public int SessionId { get; set; }
		List<ArraySegment<byte>> _reserveQueue = new List<ArraySegment<byte>>();

		// 패킷 모아 보내기
		int _reservedSendBytes = 0;
		long _lastSendTick = 0;


        #region Network
        // 예약만 하고 보내지는 않는다
        public void Send(ClientSession session, IMessage packet)
        {
            string msgName = packet.Descriptor.Name.Replace("_", string.Empty);
            MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), msgName);
            ushort size = (ushort)packet.CalculateSize();
            byte[] sendBuffer = new byte[size + 8];
            Array.Copy(BitConverter.GetBytes((int)(session.SessionId)), 0, sendBuffer, 0, sizeof(int));
            Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 4, sizeof(ushort));
            Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 6, sizeof(ushort));
            Array.Copy(packet.ToByteArray(), 0, sendBuffer, 8, size);

            lock (_lock)
            {
                _reserveQueue.Add(sendBuffer);
                _reservedSendBytes += sendBuffer.Length;
            }
        }

        // 실제 Network IO 보내는 부분
        public void FlushSend()
		{
			List<ArraySegment<byte>> sendList = null;

			lock (_lock)
			{
				// 0.1초가 지났거나, 너무 패킷이 많이 모일 때 (1만 바이트)
				long delta = (System.Environment.TickCount64 - _lastSendTick);
				if (delta < 100 && _reservedSendBytes < 10000)
					return;

				// 패킷 모아 보내기
				_reservedSendBytes = 0;
				_lastSendTick = System.Environment.TickCount64;

				sendList = _reserveQueue;
				_reserveQueue = new List<ArraySegment<byte>>();
			}

			Send(sendList);
		}

		public override void OnConnected(EndPoint endPoint)
		{
			Console.WriteLine($"OnConnected : {endPoint}");
		}

		public override void OnRecvPacket(ArraySegment<byte> buffer)
		{
			GamePacketManager.Instance.OnRecvPacket(this, buffer);
		}

		public override void OnDisconnected(EndPoint endPoint)
		{
            Debug.Assert(false, "GameSession Disconnected");
        }

        public override void OnSend(int numOfBytes)
		{
			//Console.WriteLine($"Transferred bytes: {numOfBytes}");
		}
		#endregion
	}
}
