﻿using Google.Protobuf.Protocol;
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

            // 핑 패킷 보내기
            GameLogic.Instance.PushAfter(5000, Ping);

            // 계정 추가하기
            GameAccountManager.Instance.Add(GameAccountDbId, this);

            // 유저를 GameRoom에 추가
            GameLogic.Instance.Push(() => { EnterRoom(GameLogic.Instance.Find(0)); });
        }

        private void LoginFail()
		{
            Console.WriteLine("Login Fail");

            // 로그인 실패 패킷 보내기
            S_Login sendPacket = new S_Login();
            sendPacket.LoginResult = LoginResult.LoginFail;
            Send(sendPacket);

            // 연결 끊기
            GameLogic.Instance.PushAfter(5000, Disconnect);
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

            if (MyRoom != null)
                MyRoom.LeaveRoom(this);

            GameLogic.Instance.Push(LeaveRoom);
        }

        private GameAccountDb CreateAccount(int accountDbId)
		{
            Console.WriteLine("Try create account");
			GameAccountDb gameAccount = null;

            // 만드려는 계정이 이미 있는지
            using (GameDbContext db = new GameDbContext())
			{
				gameAccount = db.Accounts
					.Where(a => a.GameAccountDbId == accountDbId).FirstOrDefault();

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

        #region Room
        public GameRoom MyRoom { get; private set; }

        // room : 입장할 GameRoom
        public void EnterRoom(GameRoom room)
        {
            if (room == null) // 입장맵이 없는 경우
                return;          

            // 현재 입장해있는 GameRoom에서 나오기
            LeaveRoom();

            // GameRoom 입장하자
            MyRoom = room;
            MyRoom.EnterRoom(this);
        }

        // 현재 입장해있는 GameRoom에서 나오기
        public void LeaveRoom()
        {
            if (MyRoom == null) // 현재 입장해있는 맵이 없는경우
                return;

            MyRoom.LeaveRoom(this);
        }
        #endregion
    }
}
