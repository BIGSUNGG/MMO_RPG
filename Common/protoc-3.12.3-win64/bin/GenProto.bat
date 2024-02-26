protoc.exe -I=./ --csharp_out=./ ./Protocol.proto 
IF ERRORLEVEL 1 PAUSE

START ../../../Server/PacketGenerator/bin/PacketGenerator.exe ./Protocol.proto
XCOPY /Y Protocol.cs "../../../Game/Assets/Scripts/Network/Packet"
XCOPY /Y Protocol.cs "../../../Server/Server/Packet"

XCOPY /Y ClientPacketManager.cs "../../../Game/Assets/Scripts/Network/Packet"
XCOPY /Y ServerPacketManager.cs "../../../Game/Assets/Scripts/Network/Packet"

XCOPY /Y ClientPacketManager.cs "../../../Server/Server/Packet"
XCOPY /Y GamePacketManager.cs "../../../Server/Server/Packet"