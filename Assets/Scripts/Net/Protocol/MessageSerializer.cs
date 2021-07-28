using System.Net.Sockets;

namespace Net.Protocol
{
	public abstract class MessageSerializer
	{
		public MsgType type { get; }

		public NetworkStream Stream { get; private set; }

		protected MessageSerializer(MsgType type)
		{
			this.type = type;
		}

		public void WithStream(NetworkStream stream)
		{
			Stream = stream;
		}

		public abstract void Decode(byte[] payload, ushort length);
		public abstract byte Encode(byte[] payload, ushort maxLength);
	}
}
