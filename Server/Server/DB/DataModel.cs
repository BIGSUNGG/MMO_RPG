using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Server.DB
{
	[Table("GameAccount")]
	public class GameAccountDb
	{
		public int GameAccountDbId { get; set; }
		public int AccountDbId { get; set; } // GameAccount를 가지고 있는 계정의 Db 아이디

		public PlayerDb Player { get; set; }
	}

	[Table("Player")]
	public class PlayerDb
	{
		public int PlayerDbId { get; set; }

        public int Hp { get; set; }
        public int Money { get; set; }
        public int MapId { get; set; }
        public List<ItemInfoDb> ItemSlot { get; set; } = new List<ItemInfoDb>();

		[ForeignKey("Account")]
		public int AccountDbId { get; set; }
		public GameAccountDb Account { get; set; }
    }

	[Table("Item")]
    public class ItemInfoDb
    {
        public int ItemInfoDbId { get; set; }

        public byte Type { get; set; }
        public int Count { get; set; }

        [ForeignKey("Player")]
        public int PlayerDbId { get; set; }
        public PlayerDb Player { get; set; }
    }
}
