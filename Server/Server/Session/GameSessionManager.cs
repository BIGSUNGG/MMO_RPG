using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
	class GameSessionManager
	{
		static GameSessionManager _session = new GameSessionManager();
		public static GameSessionManager Instance { get { return _session; } }

		int _sessionId = 0;
		Dictionary<int, GameSession> _sessions = new Dictionary<int, GameSession>();
		object _lock = new object();

		public List<GameSession> GetSessions()
		{
			List<GameSession> sessions = new List<GameSession>();

			lock (_lock)
			{
				sessions = _sessions.Values.ToList();
			}

			return sessions;
		}

		public GameSession Generate()
		{
			lock (_lock)
			{
				int sessionId = ++_sessionId;

				GameSession session = new GameSession();
                session.SessionId = sessionId;
                _sessions.Add(sessionId, session);

				Console.WriteLine($"Connected ({_sessions.Count}) Room");

				return session;
			}
		}

		public GameSession Find(int id)
		{
			lock (_lock)
			{
				GameSession session = null;
				_sessions.TryGetValue(id, out session);
				return session;
			}
		}

		public void Remove(GameSession session)
		{
			lock (_lock)
			{
				_sessions.Remove(session.SessionId);
				Console.WriteLine($"Connected ({_sessions.Count}) Players");
			}
		}
	}
}
