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
		public int AccountDbId { get; private set; }

		public void LoginAccount(C_Login loginPacket)
		{
			if (ServerState != PlayerServerState.ServerStateOffline)
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
					LoginFail(loginPacket);
					return;
				}
			}

			// 토큰에 맞는 계정 찾기
			GameAccountDb findAccount = null;
			using (GameDbContext db = new GameDbContext())
			{
				findAccount = db.Accounts
					.Where(a => a.AccountDbId == findToken.AccountDbId).FirstOrDefault();
            }

            if (findAccount == null) // 계정을 못찾았을 경우
            {
                Console.WriteLine("Failed to find account");

                // 계정 만들기
                findAccount = CreateAccount(findToken.AccountDbId);
                if (findAccount == null) // 계정을 못만들었을 경우
                {
                    LoginFail(loginPacket);
                    return;
                }
            }

            LoginSuccess(loginPacket);
		}

		private void LoginSuccess(C_Login loginPacket)
		{
            Console.WriteLine("Login Success");
            AccountDbId = loginPacket.AccountId;
            ServerState = PlayerServerState.ServerStateOnline;

            // 로그인 성공 패킷 보내기
            S_Login sendPacket = new S_Login();
            sendPacket.LoginState = LoginState.LoginSuccess;
            Send(sendPacket);

            // 핑 패킷 보내기
            GameLogic.Instance.PushAfter(5000, Ping);
        }

        private void LoginFail(C_Login loginPacket)
		{
            Console.WriteLine("Login Fail");

            // 로그인 실패 패킷 보내기
            S_Login sendPacket = new S_Login();
            sendPacket.LoginState = LoginState.LoginFail;
            Send(sendPacket);

            // 연결 끊기
            GameLogic.Instance.PushAfter(5000, Disconnect);
        }

        private void LogoutAccount()
        {
            Console.WriteLine("Logout");

            ServerState = PlayerServerState.ServerStateOffline;
        }

        private GameAccountDb CreateAccount(int accountDbId)
		{
            Console.WriteLine("Try create account");
			GameAccountDb gameAccount = null;

            // 만드려는 계정이 이미 있는지
            using (GameDbContext db = new GameDbContext())
			{
				gameAccount = db.Accounts
					.Where(a => a.AccountDbId == accountDbId).FirstOrDefault();

				if (gameAccount == null) // 만들어져있는 계정이 없으면
				{
					// 계정 만들기
					gameAccount = new GameAccountDb();
                    gameAccount.AccountDbId = accountDbId;

					db.Accounts.Add(gameAccount);
					if(db.SaveChangesEx())
                        Console.WriteLine("Create Account Succeed");
                }
                else // 계정이 이미 있으면
                    Console.WriteLine("Create Account fail");
            }

            return gameAccount;
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
		#endregion
	}
}
