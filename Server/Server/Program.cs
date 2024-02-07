using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using Server.Data;
using Server.DB;
using Server.Game;
using ServerCore;
using SharedDB;
using static System.Net.Mime.MediaTypeNames;
using System.IO;
namespace Server
{
	// 1. GameRoom 방식의 간단한 동기화 <- OK
	// 2. 더 넓은 영역 관리
	// 3. 심리스 MMO

	// 1. Recv (N개)     서빙
	// 2. GameLogic (1)  요리사
	// 3. Send (1개)     서빙
	// 4. DB (1)         결제/장부

	class Program
	{
		static Listener _listener = new Listener();

		static void GameLogicTask()
		{
			while (true)
			{
				GameLogic.Instance.Update();
				Thread.Sleep(0);
			}
		}

		static void DbTask()
		{
			while (true)
			{
				DbTransaction.Instance.Flush();
				Thread.Sleep(0);
			}
		}

		static void NetworkTask()
		{
			while (true)
			{
				List<ClientSession> sessions = ClientSessionManager.Instance.GetSessions();
				foreach (ClientSession session in sessions)
				{
					session.FlushSend();
				}

				Thread.Sleep(0);
			}
		}

		static void StartServerInfoTask()
		{
			var t = new System.Timers.Timer();
			t.AutoReset = true;
			t.Elapsed += new System.Timers.ElapsedEventHandler((s, e) =>
			{
				using (SharedDbContext shared = new SharedDbContext())
				{
					ServerDb serverDb = shared.Servers.Where(s => s.Name == Name).FirstOrDefault();
					if (serverDb != null)
					{
						serverDb.IpAddress = IpAddress;
						serverDb.Port = Port;
						serverDb.BusyScore = ClientSessionManager.Instance.GetBusyScore();
						shared.SaveChangesEx();
					}
					else
					{
						serverDb = new ServerDb()
						{
							Name = Program.Name,
							IpAddress = Program.IpAddress,
							Port = Program.Port,
							BusyScore = ClientSessionManager.Instance.GetBusyScore()
						};
						shared.Servers.Add(serverDb);
						shared.SaveChangesEx();
					}
				}
			});
			t.Interval = 10 * 1000;
			t.Start();
		}

        static void CreateGameRooms()
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[1];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7778);

            Listener listener = new Listener();
            listener.Init(endPoint, () => { return null; });

            int processCount = 2;
            // Get Unity server program path
            string processPath = Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().Length - 38) + "\\Game\\Build\\Server\\Game.exe";

            Console.WriteLine(processPath);

            for (int i = 0; i < processCount; i++)
            {
                bool bProgramConnect = false; // When unity server connect. set true

                Process process = new Process();
                GameRoom room = GameLogic.Instance.Add(i, process); // Make new game room 

                listener.SetSessionFactory(() =>
                {
                    bProgramConnect = true;

                    GameSession session = new GameSession();
                    session.Room = room;
                    room.Session = session;
                    return session;
                });

                // Make program open on new window
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.CreateNoWindow = false;
                // Set Unity server program path
                process.StartInfo.FileName = processPath;
                process.Start();

                while(true)
                { 
                    // Waiting for unity server connect
                    if(bProgramConnect)
                    {
                        Console.WriteLine("Unity server connect");
                        break;
                    }
                }
            }

        }

        static void StartServer()
        {
            // DNS (Domain Name System)
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[1];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, Port);

            IpAddress = ipAddr.ToString();

            _listener.Init(endPoint, () => { return ClientSessionManager.Instance.Generate(); });
            Console.WriteLine("Listening...");

            // DbTask
            {
                Thread t = new Thread(DbTask);
                t.Name = "DB";
                t.Start();
            }

            // NetworkTask
            {
                Thread t = new Thread(NetworkTask);
                t.Name = "Network Send";
                t.Start();
            }

            // GameLogic
            Thread.CurrentThread.Name = "GameLogic";
            GameLogicTask();
        }

        public static string Path = new string("");
        public static string Name { get; } = "데포르쥬";
		public static int Port { get; } = 7777;
		public static string IpAddress { get; set; }

		static void Main(string[] args)
		{
            // Get Project Path
            for(int i = 0; i < Environment.CurrentDirectory.Length - 38; i++)
                Path += Environment.CurrentDirectory[i];

            ConfigManager.LoadConfig();
			DataManager.LoadData();

			StartServerInfoTask();
            CreateGameRooms();
            StartServer();
        }
    }
}
