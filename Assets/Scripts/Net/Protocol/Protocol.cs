namespace Net.Protocol
{
	public enum MsgType
	{
		TypeSimImu,
		TypeSimAcc,
		TypeSimGyro,
		TypeSimMag,
		TypeSimBarometer,
	}

	public struct MsgPacket
	{
		public ushort Length;
		public MsgType Type;
		public byte[] Payload;
	}

	public abstract class Protocol
	{
		private const ushort MaxBufferSize = 1024;
		private byte[] _buffer = new byte[MaxBufferSize];
		private ushort _bufferIndex;
		private ushort _bufferLength;

		public void ReceiveStream(byte[] buf, ushort length)
		{
			for (ushort i = 0; i < length; i++)
			{
				_buffer.SetValue(buf[i], _bufferIndex);
				_bufferLength++;

				if (_decode(buf[i]))
				{
					_processPacket(_buffer, _bufferLength);
					_bufferIndex = 0;
					_bufferLength = 0;
				}
				else
				{
					if (_bufferIndex >= MaxBufferSize - 1)
					{
						_bufferIndex = 0;
						_bufferLength = 0;
					}
					else
					{
						_bufferIndex++;
					}
				}
			}
		}

		public ushort PacketStream(IMessageSerializer msg, byte[] buffer, ushort maxLength)
		{
			return 0;
		}

		protected abstract bool _decode(byte c);
		protected abstract void _processPacket(byte[] buffer, ushort length);

		protected abstract void _msgSimImuHandler(MessageSimImu msg);
	}
}
