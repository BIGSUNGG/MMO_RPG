using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountServer.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedDB;

namespace AccountServer.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AccountController : ControllerBase
	{
		AccountDbContext _context;
		SharedDbContext _shared;

		public AccountController(AccountDbContext context, SharedDbContext shared)
		{
			_context = context;
			_shared = shared;
		}

		[HttpPost]
		[Route("create")]
		public CreateAccountPacketRes CreateAccount([FromBody] CreateAccountPacketReq req)
		{
			CreateAccountPacketRes res = new CreateAccountPacketRes();

			AccountDb account = _context.Accounts
									.AsNoTracking()
									.Where(a => a.LoginId == req.AccountName)
									.FirstOrDefault();

			if (account == null)
			{
				_context.Accounts.Add(new AccountDb()
				{
                    LoginId = req.AccountName,
					Password = req.Password
				});

				bool success = _context.SaveChangesEx();
				res.CreateOk = success;

                Console.WriteLine($"Create Success Id : {req.AccountName}, Password : {req.Password}");
			}

            else
			{
				res.CreateOk = false;
                Console.WriteLine($"Create Fail Id : {req.AccountName}, Password : {req.Password}");
            }

            return res;
		}

		[HttpPost]
		[Route("login")]
		public LoginAccountPacketRes LoginAccount([FromBody] LoginAccountPacketReq req)
		{
			LoginAccountPacketRes res = new LoginAccountPacketRes();

			AccountDb account = _context.Accounts
				.AsNoTracking()
				.Where(a => a.LoginId == req.AccountName && a.Password == req.Password)
				.FirstOrDefault();

			if (account == null)
			{
                Console.WriteLine($"Create Fail Id : {req.AccountName}, Password : {req.Password}");
				res.LoginOk = false;
            }
            else
			{
                Console.WriteLine($"Create Success Id : {req.AccountName}, Password : {req.Password}");
				res.LoginOk = true;

				// 토큰 발급
				DateTime expired = DateTime.UtcNow;
				expired.AddSeconds(600);

				TokenDb tokenDb = _shared.Tokens.Where(t => t.AccountDbId == account.AccountDbId).FirstOrDefault();
				if (tokenDb != null)
				{
					tokenDb.Token = new Random().Next(Int32.MinValue, Int32.MaxValue);
					tokenDb.Expired = expired;
					_shared.SaveChangesEx();
				}
				else
				{
					tokenDb = new TokenDb()
					{
						AccountDbId = account.AccountDbId,
						Token = new Random().Next(Int32.MinValue, Int32.MaxValue),
						Expired = expired
					};
					_shared.Add(tokenDb);
					_shared.SaveChangesEx();
				}

				res.AccountId = account.AccountDbId;
				res.Token = tokenDb.Token;
				res.ServerList = new List<ServerInfo>();

				foreach (ServerDb serverDb in _shared.Servers)
				{
					res.ServerList.Add(new ServerInfo()
					{
						Name = serverDb.Name,
						IpAddress = serverDb.IpAddress,
						Port = serverDb.Port,
						BusyScore = serverDb.BusyScore
					});
				}
			}

			return res;
		}
	}
}
