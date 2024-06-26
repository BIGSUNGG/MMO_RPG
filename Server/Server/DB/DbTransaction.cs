using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Server.DB
{
	public partial class DbTransaction : JobSerializer
	{
		public static DbTransaction Instance { get; } = new DbTransaction();

        public static void SavePlayer(int gameAccountId, int mapId, PlayerInfo info)
        {
            Instance.Push(() => 
            {
                using (GameDbContext db = new GameDbContext())
                {
                    GameAccountDb findAccount = db.Accounts
                        .Include(a => a.Player)
                        .Where(a => a.GameAccountDbId == gameAccountId).FirstOrDefault();

                    if (findAccount == null)
                        return;

                    findAccount.Player.MapId = mapId;
                    findAccount.Player.Hp = info.Hp;
                    if (db.SaveChangesEx())
                        Console.WriteLine("Save Player Succeed");

                    GameAccountManager.Instance.SaveSucceed(gameAccountId);
                }
            });
        }
    }
}
