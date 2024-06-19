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
using Server.DB;
using Server.Game;
using ServerCore;
using SharedDB;
using static System.Net.Mime.MediaTypeNames;
using System.IO;
using static System.Collections.Specialized.BitVector32;

namespace Server
{
	class Program
	{
		static Listener _listener = new Listener();

		static void GameLogicTask()
		{
			while (true)
			{
				GameInstanceManager.Instance.Update();
				Thread.Sleep(1);
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

		static void ClientNetworkTask()
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

        static void GameNetworkTask()
        {
            while (true)
            {
                List<GameSession> sessions = GameSessionManager.Instance.GetSessions();
                foreach (GameSession session in sessions)
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

        static void CreateGameInstances()
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[1];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7778);

            Listener listener = new Listener();
            listener.Init(endPoint, () => { return null; });

            int processCount = 2;
            // 유니티 게임 인스턴스 실행파일 경로 구하기
            string processPath = Path + "\\Game\\Build\\Server\\Game.exe";

            for (int i = 0; i < processCount; i++)
            {
                int mapId = i;
                bool bProgramConnect = false; // 유니티 게임 인스턴스가 연결됬을때 true로 설정

                Process process = new Process();
                GameInstance instance = GameInstanceManager.Instance.Add(mapId, process); // 새로운 게임 인스턴스 만들기

                listener.SetSessionFactory(() =>
                {
                    // 세션과 게임 인스턴스 설정
                    GameSession session = GameSessionManager.Instance.Generate();
                    session.Map = instance;
                    instance.Session = session;

                    // 프로그램 연결 알리기
                    bProgramConnect = true;

                    return session;
                });

                if (bTestUnityServer == false || mapId != TestUnityServerId)
                {
                    // 새로운 윈도우 창에서 열리도록 설정
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.CreateNoWindow = false;
                    // 유니티 경로 설정
                    process.StartInfo.FileName = processPath;
                    process.Start();
                }

                while(true)
                { 
                    // 유니티 게임 인스턴스 연결 기다리기
                    if(bProgramConnect)
                    {   
                        // 게임 인스턴스로 맵 아이디 전송
                        S_EnterMap packet = new S_EnterMap();
                        packet.MapId = mapId;

                        instance.Session.Send(packet);
                        instance.Session.FlushSend();
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
                Thread t = new Thread(ClientNetworkTask);
                t.Name = "Client Network Send";
                t.Start();
            }
            {
                Thread t = new Thread(GameNetworkTask);
                t.Name = "Game Network Send";
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
        public static bool bTestUnityServer = false;
        public static int TestUnityServerId = 1;

        static void Main(string[] args)
		{
            // Get Project Path
            for(int i = 0; i < Environment.CurrentDirectory.Length - 38; i++)
                Path += Environment.CurrentDirectory[i];

			StartServerInfoTask();
            CreateGameInstances();
            StartServer();
        }
    }
}
