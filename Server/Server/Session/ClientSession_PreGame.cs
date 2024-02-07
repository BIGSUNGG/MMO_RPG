using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DB;
using Server.Game;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
	public partial class ClientSession : PacketSession
	{
		public int AccountDbId { get; private set; }
		public List<LobbyPlayerInfo> LobbyPlayers { get; set; } = new List<LobbyPlayerInfo>();

		public void HandleLogin(C_Login loginPacket)
		{
			// TODO : 이런 저런 보안 체크
			if (ServerState != PlayerServerState.ServerStateLogin)
				return;

			// TODO : 문제가 있긴 있다
			// - 동시에 다른 사람이 같은 UniqueId을 보낸다면?
			// - 악의적으로 여러번 보낸다면
			// - 쌩뚱맞은 타이밍에 그냥 이 패킷을 보낸다면?

			LobbyPlayers.Clear();

			using (AppDbContext db = new AppDbContext())
			{
				AccountDb findAccount = db.Accounts
					.Include(a => a.Players)
					.Where(a => a.AccountName == loginPacket.UniqueId).FirstOrDefault();

				if (findAccount != null)
				{
					// AccountDbId 메모리에 기억
					AccountDbId = findAccount.AccountDbId;

					S_Login loginOk = new S_Login() { LoginOk = 1 };
					foreach (PlayerDb playerDb in findAccount.Players)
					{
						LobbyPlayerInfo lobbyPlayer = new LobbyPlayerInfo()
						{
							PlayerDbId = playerDb.PlayerDbId,
							Name = playerDb.PlayerName,
						};

						// 메모리에도 들고 있다
						LobbyPlayers.Add(lobbyPlayer);

						// 패킷에 넣어준다
						loginOk.Players.Add(lobbyPlayer);
					}

					Send(loginOk);
					// 로비로 이동
					ServerState = PlayerServerState.ServerStateLobby;
				}
				else
				{
					AccountDb newAccount = new AccountDb() { AccountName = loginPacket.UniqueId };
					db.Accounts.Add(newAccount);
					bool success = db.SaveChangesEx();
					if (success == false)
						return;

					// AccountDbId 메모리에 기억
					AccountDbId = newAccount.AccountDbId;

					S_Login loginOk = new S_Login() { LoginOk = 1 };
					Send(loginOk);
					// 로비로 이동
					ServerState = PlayerServerState.ServerStateLobby;
				}
			}
		}

		public void HandleEnterGame(C_EnterGame enterGamePacket)
		{
			if (ServerState != PlayerServerState.ServerStateLobby)
				return;

			LobbyPlayerInfo playerInfo = LobbyPlayers.Find(p => p.Name == enterGamePacket.Name);
			if (playerInfo == null)
				return;

            MyPlayer = ObjectManager.Instance.Add<Player>();

            ServerState = PlayerServerState.ServerStateGame;

			GameLogic.Instance.Push(() =>
			{
				GameRoom room = GameLogic.Instance.Find(1);
				room.Push(room.EnterGame, MyPlayer);
			});
		}

		public void HandleCreatePlayer(C_CreatePlayer createPacket)
		{
			// TODO : 이런 저런 보안 체크
			if (ServerState != PlayerServerState.ServerStateLobby)
				return;
		}
	}
}
