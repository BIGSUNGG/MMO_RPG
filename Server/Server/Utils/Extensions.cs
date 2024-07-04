﻿using Server.DB;
using SharedDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
	public static class Extensions
	{
		public static bool SaveChangesEx(this GameDbContext db)
		{
			try
			{
				db.SaveChanges();
				return true;
			}
            catch (System.Exception ex)
            {
				Console.WriteLine(ex);
                return false;
			}
		}

		public static bool SaveChangesEx(this SharedDbContext db)
		{
			try
			{
				db.SaveChanges();
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
