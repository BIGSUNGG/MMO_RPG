using System;
using System.Collections.Generic;
using System.Text;

namespace PacketGenerator
{
    class PacketFormat
    {
        #region Manager
        // {0] 클래스 이름
        // {1} 패킷 등록
        public static string managerFormat =
@"using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;

class {0}PacketManager
{{
	#region Singleton
	static {0}PacketManager _instance = new {0}PacketManager();
	public static {0}PacketManager Instance {{ get {{ return _instance; }} }}
	#endregion

	{0}PacketManager()
	{{
		Register();
	}}

	Dictionary<ushort, Action<ISession, ArraySegment<byte>, ushort>> _onRecv = new Dictionary<ushort, Action<ISession, ArraySegment<byte>, ushort>>();
	Dictionary<ushort, Action<ISession, IMessage>> _handler = new Dictionary<ushort, Action<ISession, IMessage>>();
		
	public Action<ISession, IMessage, ushort> CustomHandler {{ get; set; }}

	public void Register()
	{{{1}
	}}

	public void OnRecvPacket(ISession session, ArraySegment<byte> buffer)
	{{
		ushort count = 0;

		ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
		count += 2;
		ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
		count += 2;

		Action<ISession, ArraySegment<byte>, ushort> action = null;
		if (_onRecv.TryGetValue(id, out action))
			action.Invoke(session, buffer, id);
	}}

	void MakePacket<T>(ISession session, ArraySegment<byte> buffer, ushort id) where T : IMessage, new()
	{{
		T pkt = new T();
		pkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);

		if (CustomHandler != null)
		{{
			CustomHandler.Invoke(session, pkt, id);
		}}
		else
		{{
			Action<ISession, IMessage> action = null;
			if (_handler.TryGetValue(id, out action))
				action.Invoke(session, pkt);
		}}
	}}

	public Action<ISession, IMessage> GetPacketHandler(ushort id)
	{{
		Action<ISession, IMessage> action = null;
		if (_handler.TryGetValue(id, out action))
			return action;
		return null;
	}}
}}";

        // {0} 클래스 이름
        // {1} MsgId
        // {2} 패킷 이름
        public static string managerRegisterFormat =
@"		
		_onRecv.Add((ushort)MsgId.{1}, MakePacket<{2}>);
		_handler.Add((ushort)MsgId.{1}, {0}PacketHandler.{2}Handler);";
        #endregion
    }
}