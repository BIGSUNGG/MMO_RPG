using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Server.Game
{
	public class GameObject
	{
		public GameRoom Room { get; set; }

        public GameObject()
		{

		}

		public virtual GameObject GetOwner()
		{
			return this;
		}
	}
}
