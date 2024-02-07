
using System;
using System.IO;
using System.Reflection;
using System.Xml;

namespace PacketGenerator
{
	class Program
	{
		static string clientRegister;
        static string serverRegister;
        static string gameRegister;
        
        static void Main(string[] args)
		{
			string file = "../../../Common/protoc-3.12.3-win64/bin/Protocol.proto";
			if (args.Length >= 1)
				file = args[0];

			bool startParsing = false;
			foreach (string line in File.ReadAllLines(file))
			{
				if (!startParsing && line.Contains("enum MsgId"))
				{
					startParsing = true;
					continue;
				}

				if (!startParsing)
					continue;

				if (line.Contains("}"))
					break;

				string[] names = line.Trim().Split(" =");
				if (names.Length == 0)
					continue;

				string name = names[0];
				if (name.StartsWith("S_"))
				{
                    // Register
					string[] words = name.Split("_");

					string msgName = "";
					foreach (string word in words)
						msgName += FirstCharToUpper(word);

					string packetName = $"S_{msgName.Substring(1)}";
					clientRegister += string.Format(PacketFormat.managerRegisterFormat, "Server", msgName, packetName);

                }
				else if (name.StartsWith("C_"))
				{
                    // Register
					string[] words = name.Split("_");

                    string msgName = "";
					foreach (string word in words)
						msgName += FirstCharToUpper(word);

					string packetName = $"C_{msgName.Substring(1)}";
					serverRegister += string.Format(PacketFormat.managerRegisterFormat, "Client", msgName, packetName);

                }
                else if (name.StartsWith("G_"))
                {
                    // Register
                    string[] words = name.Split("_");

                    string msgName = "";
                    foreach (string word in words)
                        msgName += FirstCharToUpper(word);

                    string packetName = $"G_{msgName.Substring(1)}";
                    gameRegister += string.Format(PacketFormat.managerRegisterFormat, "Game", msgName, packetName);

                }
            }

			string clientManagerText = string.Format(PacketFormat.managerFormat, "Server", clientRegister);
			File.WriteAllText("ServerPacketManager.cs", clientManagerText);

            string serverManagerText = string.Format(PacketFormat.managerFormat, "Client", serverRegister);
			File.WriteAllText("ClientPacketManager.cs", serverManagerText);

            string gameManagerText = string.Format(PacketFormat.managerFormat, "Game", gameRegister);
            File.WriteAllText("GamePacketManager.cs", gameManagerText);
        }

		public static string FirstCharToUpper(string input)
		{
			if (string.IsNullOrEmpty(input))
				return "";
			return input[0].ToString().ToUpper() + input.Substring(1).ToLower();
		}
	}
}
