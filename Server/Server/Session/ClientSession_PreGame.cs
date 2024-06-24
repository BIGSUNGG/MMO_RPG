using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.DB;
using Server.Game;
using ServerCore;
using SharedDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
	public partial class ClientSession : PacketSession
	{
        #region Account
        public int GameAccountDbId { get; private set; }
        public PlayerLoginState LoginState { get; private set; } = PlayerLoginState.NotLoggedIn;

        // 새로운 계정과 플레이어 정보를 만듦
        private GameAccountDb CreateAccount(int accountDbId)
		{
            Console.WriteLine("Try create account");
			GameAccountDb gameAccount = null;

            // 만드려는 계정이 이미 있는지
            using (GameDbContext db = new GameDbContext())
			{
				gameAccount = db.Accounts
                    .Include(a => a.Player)
					.Where(a => a.GameAccountDbId == accountDbId).FirstOrDefault();

				if (gameAccount == null) // 만들어져있는 계정이 없으면
				{
					// 계정 만들기
					gameAccount = new GameAccountDb();
                    gameAccount.AccountDbId = accountDbId;

                    db.Accounts.Add(gameAccount);

                    if (db.SaveChangesEx())
                        Console.WriteLine("Create Account Succeed");
                    else
                        Console.WriteLine("Create Account fail");
                }
                
                if(gameAccount.Player == null) // 계정의 플레이어 정보가 없으면
                {
                    CreatePlayer(accountDbId);
                }
            }

            return gameAccount;
		}

        private PlayerDb CreatePlayer(int accountDbId)
        {
            Console.WriteLine("Try create player");
            GameAccountDb findAccount = null;
            PlayerDb createAccount = null;

            using (GameDbContext db = new GameDbContext())
            {
                findAccount = db.Accounts
                    .Include(a => a.Player)
                    .Where(a => a.GameAccountDbId == accountDbId).FirstOrDefault();

                if(findAccount == null)
                {
                    findAccount = CreateAccount(accountDbId);
                }

                if (findAccount.Player == null) // 계정의 플레이어 정보가 없으면
                {
                    findAccount.Player = new PlayerDb();
                    findAccount.Player.Hp = 100;
                    findAccount.Player.MapId = 0;

                    createAccount = findAccount.Player;
                    db.Players.Add(createAccount);

                    if (db.SaveChangesEx())
                        Console.WriteLine("Create Player Succeed");
                    else
                        Console.WriteLine("Create Player fail");
                }
            }

            return createAccount;
        }

		private bool DeleteAccount(int accountDbId)
		{
			using (GameDbContext db = new GameDbContext())
			{
                GameAccountDb findAccount = db.Accounts
                    .Where(a => a.AccountDbId == accountDbId).FirstOrDefault();

                if(findAccount != null)
                {
                    db.Remove(findAccount);
                    return true;
                }
			}

			return false;
		}

        public void LoginAccount(C_Login loginPacket)
		{
			if (LoginState != PlayerLoginState.NotLoggedIn)
				return;

            Console.WriteLine("Login Try");

			// 패킷에 맞는 토큰 찾기
			TokenDb findToken = null;
			using (SharedDbContext db = new SharedDbContext())
			{
				findToken = db.Tokens
					.Where(t => t.Token == loginPacket.Token && t.AccountDbId == loginPacket.AccountId).FirstOrDefault();

				if (findToken == null) // 토큰을 못찾았을 경우
				{
                    Console.WriteLine("Failed to find token");
					LoginFail();
					return;
				}
			}

			// 토큰에 맞는 계정 찾기
			GameAccountDb findAccount = null;
			using (GameDbContext db = new GameDbContext())
			{
				findAccount = db.Accounts
					.Where(a => a.GameAccountDbId == findToken.AccountDbId).FirstOrDefault();
            }

            if (findAccount == null) // 계정을 못찾았을 경우
            {
                Console.WriteLine("Failed to find account");

                // 계정 만들기
                findAccount = CreateAccount(findToken.AccountDbId);
                if (findAccount == null) // 계정을 못만들었을 경우
                {
                    LoginFail();
                    return;
                }
            }
            else // 계정이 있을 경우
            {
                ClientSession session = GameAccountManager.Instance.Find(findAccount.GameAccountDbId);
                if (session != null && session.LoginState == PlayerLoginState.LoggedIn) // 찾은 계정이 다른 클라이언트에서 이미 플레이 중이라면
                {
                    Console.WriteLine("Already playing this account");
                    session.Disconnect(); // 이미 연결중인 클라이언트 연결해제
                }
            }

            LoginSuccess(findAccount.GameAccountDbId);
		}

		private void LoginSuccess(int gameAccountDbId)
		{
            Console.WriteLine("Login Success");
            GameAccountDbId = gameAccountDbId;
            LoginState = PlayerLoginState.LoggedIn;

            // 로그인 성공 패킷 보내기
            S_Login sendPacket = new S_Login();
            sendPacket.LoginResult = LoginResult.LoginSuccess;
            Send(sendPacket);

            // 계정 정보에 맞게 세션 정보 업데이트
            GameAccountDb loginAccount = null;
            using (GameDbContext db = new GameDbContext())
            {
                loginAccount = db.Accounts
                    .AsNoTracking()
                    .Include(a => a.Player)
                    .Where(a => a.GameAccountDbId == gameAccountDbId).FirstOrDefault();

                if(loginAccount == null)
                {
                    Console.WriteLine("Login account is null");
                    return;
                }       
                
                if(loginAccount.Player == null)
                {
                    Console.WriteLine("Login account's player is null");
                    loginAccount.Player = CreatePlayer(gameAccountDbId);
                }
            }

            // 플레이어 정보 업데이트
            PlayerInfo info = new PlayerInfo();
            {
                info.SessionId = SessionId;
                info.Hp = loginAccount.Player.Hp;
                SetPlayerInfo(info);
            }

            // 접속중인 계정 추가하기
            GameAccountManager.Instance.Add(GameAccountDbId, this);

            // 핑 패킷 보내기
            GameInstanceManager.Instance.PushAfter(5000, Ping);

            // 유저를 GameMap에 추가
            GameInstanceManager.Instance.Push(() => { EnterMap(GameInstanceManager.Instance.Find(loginAccount.Player.MapId)); });
        }

        private void LoginFail()
		{
            Console.WriteLine("Login Fail");

            // 로그인 실패 패킷 보내기
            S_Login sendPacket = new S_Login();
            sendPacket.LoginResult = LoginResult.LoginFail;
            Send(sendPacket);

            // 연결 끊기
            GameInstanceManager.Instance.PushAfter(5000, Disconnect);
        }

        private void LogoutAccount()
        {
            Console.WriteLine("Logout");

            LoginState = PlayerLoginState.NotLoggedIn;

            // 계정 제거하기
            if(LoginState == PlayerLoginState.LoggedIn) // 이미 로그인 중이라면
            {
                GameAccountManager.Instance.Remove(GameAccountDbId);                
            }

            if (MyMap != null)
            {
                S_RequestPlayerInfo requestPacket = new S_RequestPlayerInfo();
                requestPacket.SessionId = SessionId;
                requestPacket.GameAccountId = GameAccountDbId;
                MyMap.Session.Send(requestPacket);

                MyMap.LeaveMap(this);
            }

            GameInstanceManager.Instance.Push(LeaveMap);
        }

        #endregion

        #region Map
        public GameInstance MyMap { get; private set; }

        // map : 입장할 GameMap
        public void EnterMap(int mapId)
        {
            EnterMap(GameInstanceManager.Instance.Find(mapId));
        }

        public void EnterMap(GameInstance map)
        {
            if (map == null) // 입장맵이 없는 경우
                return;          
            
            // 현재 입장해있는 GameMap에서 나오기
            LeaveMap();

            // GameMap 입장하기
            MyMap = map;
            MyMap.EnterMap(this);
        }

        // 현재 입장해있는 GameMap에서 나오기
        public void LeaveMap()
        {
            if (MyMap == null) // 현재 입장해있는 맵이 없는경우
                return;

            MyMap.LeaveMap(this);
        }
        #endregion

        #region Player
        public int Hp = 100;

        public PlayerInfo GetPlayerInfo()
        {
            PlayerInfo result = new PlayerInfo();
            result.SessionId  = this.SessionId;
            result.Hp         = this.Hp;

            return result;
        }

        public void SetPlayerInfo(PlayerInfo info)
        {
            if (info.SessionId != this.SessionId)
            {
                Console.WriteLine("This player info is not this session's");
                return;
            }

            if (info.Hp == 0)
                info.Hp = 100;

            Hp = info.Hp;
        }

        #endregion
    }
}
