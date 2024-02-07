using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Server.Game
{
	public class GameObject
	{
		public GameObjectType ObjectType { get; protected set; } = GameObjectType.None;
		public int Id
		{
			get { return Info.ObjectId; }
			set { Info.ObjectId = value; }
		}

		public GameRoom Room { get; set; }

		public ObjectInfo Info { get; set; } = new ObjectInfo();

		public GameObject()
		{

		}

		public virtual void Update()
		{

		}

		public virtual void OnDamaged(GameObject attacker, int damage)
		{
		}

		public virtual void OnDead(GameObject attacker)
		{
		}

		public virtual GameObject GetOwner()
		{
			return this;
		}
	}
}
