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
                        .ThenInclude(p => p.ItemSlot)
                        .Where(a => a.GameAccountDbId == gameAccountId).FirstOrDefault();

                    if (findAccount == null)
                    {
                        Console.WriteLine("Failed to find account");
                        return;
                    }

                    PlayerDb findPlayer = findAccount.Player;

                    findPlayer.MapId = mapId;
                    findPlayer.Hp = info.Hp;
                    findPlayer.Money = info.Money;

                    if (info.ItemSlot == null)
                    {
                        findPlayer.ItemSlot = new List<ItemInfoDb>();           
                    }
                    else
                    {
                        findPlayer.ItemSlot = new List<ItemInfoDb>(new ItemInfoDb[info.ItemSlot.Count]);
                        for (int i = 0; i < info.ItemSlot.Count; i++)
                        {
                            ItemInfoDb itemInfo = new ItemInfoDb();
                            itemInfo.Player = findPlayer;
                            itemInfo.PlayerDbId = findPlayer.PlayerDbId;

                            if (info.ItemSlot[i] == null)
                            {
                                itemInfo.Type = ItemType.None;
                                itemInfo.Count = 0;
                            }
                            else
                            {
                                itemInfo.Type = info.ItemSlot[i].Type;
                                itemInfo.Count = info.ItemSlot[i].Count;
                            }
                            findPlayer.ItemSlot[i] = itemInfo;
                        }
                    }

                    if (db.SaveChangesEx())
                    {
                        Console.WriteLine($"Map : {findAccount.Player.MapId}, Hp : {findAccount.Player.Hp}, Money : {findAccount.Player.Money}");
                        Console.WriteLine("Save Player Succeed");
                    }
                    else
                    {
                        Console.WriteLine("Save Player Fail");
                    }

                    GameAccountManager.Instance.SuccessSave(gameAccountId);
                }
            });
        }
    }
}
