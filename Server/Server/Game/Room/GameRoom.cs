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
        public GameSession Session { get; set; }
        public Process Program { get; set; }
        public int RoomId { get; set; }
        public int MapId { get; set; }

        object _lock = new object();


        public void Init(int mapId, int zoneCells)
        {

        }

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
                ClientSession findSession = _sessions[session.GameAccountDbId];
                if (findSession != null)
                    Console.WriteLine($"{session.GameAccountDbId} Account is already in {RoomId} Room");

                findSession = session;
            }
        }

        public ClientSession FindSession(int id)
        {
            lock (_lock)
            {
                ClientSession result;
                _sessions.TryGetValue(id, out result);
                return result;
            }
        }

        public bool LeaveRoom(ClientSession session)
        {
            lock(_lock)
            {
                return _sessions.Remove(session.GameAccountDbId);
            }
        }

        public void DoActionAll(Func<ArraySegment<byte>> packet)
        {
            lock(_lock)
            {
	            foreach(var tuple in _sessions)
	            {
                    tuple.Value.Send(packet.Invoke());
                }
            }
        }
        #endregion
	}
}
