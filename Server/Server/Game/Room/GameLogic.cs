using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Server.Game
{
	public class GameLogic : JobSerializer
	{
		public static GameLogic Instance { get; } = new GameLogic();

		Dictionary<int, GameRoom> _rooms = new Dictionary<int, GameRoom>(); // Key : MapId, Value : GameRoom

        public void Update()
		{
			Flush();

			foreach (GameRoom room in _rooms.Values)
            {
                room.Update();
			}
		}

		public GameRoom Add(int mapId, Process program)
		{
			GameRoom gameRoom = new GameRoom();
			gameRoom.MapId = mapId;
            gameRoom.Program = program;

			_rooms.Add(mapId, gameRoom);
			return gameRoom;
		}

		public bool Remove(int roomId)
		{
			return _rooms.Remove(roomId);
		}

		public GameRoom Find(int roomId)
		{
			GameRoom room = null;
			if (_rooms.TryGetValue(roomId, out room))
				return room;

			return null;
		}
	}
}
