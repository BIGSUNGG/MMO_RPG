using ServerCore;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Server
{
    // 접속 중인 계정들을 관리하는 클래스
    public class GameAccountManager
    {
        public static GameAccountManager Instance { get; } = new GameAccountManager();
        Dictionary<int, ClientSession> Accounts = new Dictionary<int, ClientSession>(); // Key : GameAccountDbId, Value : 플레이어의 ClientSession

        object _lock = new object();

        // gameAccountDbId : 추가할 클라이언트 세션의 게임 계정 아이디
        // addSession : 추가할 클라이언트 세션
        public void Add(int gameAccountDbId, ClientSession addSession)
        {
            lock (_lock)
            {
                Accounts[gameAccountDbId] = addSession;
            }
        }

        // gameAccountDbId : 찾을 클라이언트 세션의 게임 계정 아이디
        public ClientSession Find(int gameAccountDbId)
        {
            lock (_lock)
            {
                ClientSession session = null;
                Accounts.TryGetValue(gameAccountDbId, out session);
                return session;
            }
        }

        // gameAccountDbId : 제거할 클라이언트 세션의 게임 계정 아이디
        // return : 제거 성공 여부
        public bool Remove(int gameAccountDbId)
        {
            lock (_lock)
            {
                return Accounts.Remove(gameAccountDbId);
            }
        }
    }
}
