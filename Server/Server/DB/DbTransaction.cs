﻿using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.DB
{
	public partial class DbTransaction : JobSerializer
	{
		public static DbTransaction Instance { get; } = new DbTransaction();

	}
}
