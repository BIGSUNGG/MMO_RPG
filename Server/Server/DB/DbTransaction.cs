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

                    if (findPlayer.ItemSlot == null)
                        findPlayer.ItemSlot = new List<ItemInfoDb>();

                    // 아이템 슬롯 초기화                   
                    for (int i = 0; i < findPlayer.ItemSlot.Count; i++)
                    {
                        ItemInfoDb itemInfo = findPlayer.ItemSlot[i];
                        if (itemInfo == null)
                            itemInfo = new ItemInfoDb();

                        itemInfo.Player = findPlayer;
                        itemInfo.PlayerDbId = findPlayer.PlayerDbId;
                        itemInfo.Type = (byte)ItemType.None;
                        itemInfo.Count = 0;

                        findPlayer.ItemSlot[i] = itemInfo;
                    }

                    if (info.ItemSlot != null) // 아이템 슬롯이 있다면
                    {
                        // Db에 있는 아이템 슬롯이 저장할 아이템 슬롯보다 크다면 Db 아이템 슬롯 사이즈 줄이기
                        while(findPlayer.ItemSlot.Count > info.ItemSlot.Count)
                        {
                            findPlayer.ItemSlot.RemoveAt(findPlayer.ItemSlot.Count - 1);
                        }

                        // Db에 있는 아이템 슬롯이 저장할 아이템 슬롯보다 작다면 Db 아이템 슬롯 사이즈 늘리기
                        while (findPlayer.ItemSlot.Count < info.ItemSlot.Count)
                        {
                            ItemInfoDb itemInfo = itemInfo = new ItemInfoDb();
                            itemInfo.Player = findPlayer;
                            itemInfo.PlayerDbId = findPlayer.PlayerDbId;
                            itemInfo.Type = (byte)ItemType.None;
                            itemInfo.Count = 0;

                            findPlayer.ItemSlot.Add(itemInfo);
                            Console.WriteLine("Add");
                        }

                        // Db에 있는 아이템 슬롯 정보 수정
                        for (int i = 0; i < info.ItemSlot.Count; i++)
                        {
                            ItemInfoDb itemInfo = findPlayer.ItemSlot[i];
                            itemInfo.Player = findPlayer;
                            itemInfo.PlayerDbId = findPlayer.PlayerDbId;

                            if (info.ItemSlot[i] == null)
                            {
                                itemInfo.Type = (byte)ItemType.None;
                                itemInfo.Count = 0;
                            }
                            else
                            {
                                itemInfo.Type = (byte)info.ItemSlot[i].Type;
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
