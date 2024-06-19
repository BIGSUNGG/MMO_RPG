using System;
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
		public string PlayerName { get; set; }

		[ForeignKey("Account")]
		public int AccountDbId { get; set; }
		public GameAccountDb Account { get; set; }
    }
}
