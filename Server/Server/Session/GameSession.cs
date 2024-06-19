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
using System.Diagnostics;
using static System.Collections.Specialized.BitVector32;

namespace Server
{
	public partial class GameSession : PacketSession
    {
        protected override ushort ParseDataSize(ArraySegment<byte> buffer)
        {
            return BitConverter.ToUInt16(buffer.Array, buffer.Offset + 4);
        }

        public GameInstance Map { get; set; }

		object _lock = new object();
		public int SessionId { get; set; }
		List<ArraySegment<byte>> _reserveQueue = new List<ArraySegment<byte>>();

		// 패킷 모아 보내기
		int _reservedSendBytes = 0;
		long _lastSendTick = 0;


		#region Network
		// Send client packet
		private void Send(int Id, IMessage packet)
		{
			string msgName = packet.Descriptor.Name.Replace("_", string.Empty);
			MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), msgName);
			ushort size = (ushort)packet.CalculateSize();
			byte[] sendBuffer = new byte[size + 8];
			Array.Copy(BitConverter.GetBytes((int)Id), 0, sendBuffer, 0, sizeof(int));
			Array.Copy(BitConverter.GetBytes((ushort)(size + 8)), 0, sendBuffer, 4, sizeof(ushort));
			Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 6, sizeof(ushort));
			Array.Copy(packet.ToByteArray(), 0, sendBuffer, 8, size);

			lock (_lock)
			{
				_reserveQueue.Add(sendBuffer);
				_reservedSendBytes += sendBuffer.Length;
                #if false
                {
			        Console.WriteLine
				    	(
	                        "Send To Map : " + 
	                        "Id : " + Id +
	                        ", Packet Size : " + (ushort)(size) +
	                        ", All Size : " + (ushort)(size + 8) +
	                        ", MsgId : " + (ushort)msgId
	                    );
		        }
                #endif
			}
		}

		public void Send(ClientSession session, IMessage packet)
		{
			Send(session.SessionId, packet);
		}

		// Send server packet 
		public void Send(IMessage packet)
		{
			Send(0, packet);
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
            int id = BitConverter.ToInt32(buffer.Array, buffer.Offset);
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset + 4);
            ushort msg = BitConverter.ToUInt16(buffer.Array, buffer.Offset + 6);

#if false // Log Packet Info
            Console.WriteLine(
                "Receive From Map: " + 
                "Id : " +  id +
                ", Size : " + size +
                ", MsgId : " + msg 
                );
#endif

            int sessionId = BitConverter.ToInt32(buffer.Array, buffer.Offset);
            ArraySegment<byte> recvBuffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + 4, buffer.Count - 4);
            ushort recvBuffSize = (ushort)(size - 4);
            byte[] recvBuffSizeByte = BitConverter.GetBytes(recvBuffSize);
            recvBuffer[0] = recvBuffSizeByte[0];
            recvBuffer[1] = recvBuffSizeByte[1];

            if (sessionId == 0) // 게임 패킷을 받았을경우
            {
                GamePacketManager.Instance.OnRecvPacket(this, recvBuffer);
            }
            else if(sessionId == -1) // 모든 클라이언트로 보낼 패킷을 받았을 경우
            {
                //Console.WriteLine("Send All");

                byte[] sendBuffer = new byte[recvBuffSize];
                for (int i = 0; i < recvBuffSize; i++)
                    sendBuffer[i] = recvBuffer[i];

                Map.SendAll(sendBuffer);
                
            }
            else // 클라이언트로 보낼 패킷을 받았을 경우
            {
                //패킷을 받을 클라이언트 찾기
                ClientSession session = Map.FindSession(sessionId);

                if (session != null) // 세션을 찾았는지
                {
                    //Console.WriteLine($"Send to {sessionId} session");

                    byte[] sendBuffer = new byte[recvBuffSize];
                    for (int i = 0; i < recvBuffSize; i++)
                        sendBuffer[i] = recvBuffer[i];

                    Map.Push(() => { session.Send(sendBuffer); });                  
                }
                else
                {
                    Console.WriteLine("Recv null session's id");
                }
            }

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
