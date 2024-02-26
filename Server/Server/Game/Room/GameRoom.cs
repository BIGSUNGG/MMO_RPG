using Google.Protobuf;
using Google.Protobuf.Protocol;
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

		Dictionary<int, Player> _players = new Dictionary<int, Player>();

		public void Init(int mapId, int zoneCells)
		{

		}

		// 누군가 주기적으로 호출해줘야 한다
		public void Update()
		{
			Flush();
		}
	}
}
