using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Server.Game
{
	public class GameInstanceManager : JobSerializer
	{
		public static GameInstanceManager Instance { get; } = new GameInstanceManager();

		Dictionary<int, GameInstance> _gameInstances = new Dictionary<int, GameInstance>(); // Key : MapId, Value : GameMap

        public void Update()
		{
			Flush();

			foreach (GameInstance map in _gameInstances.Values)
            {
                map.Update();
			}
		}

		public GameInstance Add(int mapId, Process program)
		{
			GameInstance gameMap = new GameInstance();
			gameMap.MapId = mapId;
            gameMap.Program = program;

			_gameInstances.Add(mapId, gameMap);
			return gameMap;
		}

		public bool Remove(int mapId)
		{
			return _gameInstances.Remove(mapId);
		}

		public GameInstance Find(int mapId)
		{
			GameInstance map = null;
			if (_gameInstances.TryGetValue(mapId, out map))
				return map;

            Console.WriteLine($"Failed to find {mapId} map");
			return null;
		}
	}
}
