using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Server.Game
{
    public partial class GameRoom : JobSerializer
    {
        public GameSession _gameSession { get; set; }
        public Process Program { get; set; }
        public int MapId { get; set; }

        object _lock = new object();

        // 누군가 주기적으로 호출해줘야 한다
        public void Update()
        {
            Flush();
        }

        #region Sessions
        Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>(); // Key : GameAccountDbId, Value : DbId에 맞는 ClientSession

        public void EnterRoom(ClientSession session)
        {
            lock(_lock)
            {
                Console.WriteLine("EnterRoom");

                // 현재 방에 있는 세션 추가하기
                _sessions.Add(session.GameAccountDbId, session);

                // 클라이언트에게 맵 입장 알리기
                S_EnterMap enterMapPacket = new S_EnterMap();
                enterMapPacket.MapId = this.MapId;
                session.Send(enterMapPacket);

                // 모든 클라이언트와 GameRoom에 플레이어 입장 알리기
                S_EnterPlayer enterPlayerPacket = new S_EnterPlayer();
                enterPlayerPacket.SessionId = session.SessionId;
                enterPlayerPacket.AccountDbId = session.GameAccountDbId;

                _gameSession.Send(enterPlayerPacket);
                SendAll(enterPlayerPacket);

            }
        }   

        public ClientSession FindSession(int id)
        {
            lock (_lock)
            {
                ClientSession result;
                _sessions.TryGetValue(id, out result);

                if(result == null)
                    Console.WriteLine($"Find Session Failed{id}");

                return result;
            }
        }

        public bool LeaveRoom(ClientSession session)
        {
            lock(_lock)
            {
                Console.WriteLine("LeaveRoom");

                if(_sessions.Remove(session.GameAccountDbId))
                {
                    Console.WriteLine("LeaveRoom Succeed");

                    // GameRoom과 클라이언트에게 맵 퇴장 알리기
                    S_EnterMap enterMapPacket = new S_EnterMap();
	                enterMapPacket.MapId = this.MapId;
	                session.Send(enterMapPacket);
                    SendAll(enterMapPacket);

                    return true;
                }
                return false;
            }
        }

        public void DoActionAlla(Func<ArraySegment<byte>> action)
        {
            lock(_lock)
            {
	            foreach(var tuple in _sessions)
	            {
                    tuple.Value.Send(action.Invoke());
                }
            }
        }

        // 이 Room에 있는 모든 클라이언트에게 패킷 전송
        public void SendAll(IMessage packet)
        {
            string msgName = packet.Descriptor.Name.Replace("_", string.Empty);
            MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), msgName);
            ushort size = (ushort)packet.CalculateSize();
            byte[] sendBuffer = new byte[size + 4];
            Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
            Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));
            Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);

            SendAll(sendBuffer);
        }

        public void SendAll(ArraySegment<byte> packet)
        {
            lock (_lock)
            {
                foreach (var tuple in _sessions)
                {
                    tuple.Value.Send(packet);
                }
            }
        }
        #endregion
    }
}
