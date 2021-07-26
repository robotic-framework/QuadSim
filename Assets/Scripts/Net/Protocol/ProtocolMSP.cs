using System;

namespace Net.Protocol
{
	public enum MspProtocolState
	{
		MspIdle,
		MspHeaderStart,
		MspHeaderM,
		MspHeaderArrow,
		MspHeaderSize,
		MspHeaderCmd
	};

	public class ProtocolMsp : Protocol
	{
		private MspProtocolState _state;
		private ushort _dataSize;
		private ushort _dataOffset;
		private ushort _checksum;

		protected override bool _decode(byte c)
		{
			switch (_state)
			{
				case MspProtocolState.MspIdle:
				{
					// expect '$'
					if (c == '$')
					{
						_state = MspProtocolState.MspHeaderStart;
					}

					break;
				}
				case MspProtocolState.MspHeaderStart:
				{
					// expect 'M'
					_state = (c == 'M') ? MspProtocolState.MspHeaderM : MspProtocolState.MspIdle;
					break;
				}
				case MspProtocolState.MspHeaderM:
				{
					// expect '<'
					_state = (c == '<') ? MspProtocolState.MspHeaderArrow : MspProtocolState.MspIdle;
					break;
				}
				case MspProtocolState.MspHeaderArrow:
				{
					// expect size
					if (c > 64)
					{
						// invalid size
						_state = MspProtocolState.MspIdle;
					}
					else
					{
						_dataSize = c;
						_checksum = c;
						_dataOffset = 0;
						_state = MspProtocolState.MspHeaderSize;
					}

					break;
				}
				case MspProtocolState.MspHeaderSize:
				{
					// expect cmd
					_checksum ^= c;
					_state = MspProtocolState.MspHeaderCmd;
					break;
				}
				case MspProtocolState.MspHeaderCmd:
				{
					if (_dataOffset < _dataSize)
					{
						_checksum ^= c;
						_dataOffset++;
					}
					else
					{
						// the last byte is checksum, check if equal
						if (_checksum == c)
						{
							return true;
						}

						_state = MspProtocolState.MspIdle;
					}

					break;
				}
			}

			return false;
		}

		protected override void _processPacket(byte[] buffer, ushort length)
		{
			MsgPacket packet;
			packet.Payload = new byte[] { };

			// copy payload
			Array.Copy(buffer, 5, packet.Payload, 0, length - 5);

			packet.Length = buffer[3];
			packet.Type = (MsgType) buffer[4];

			switch (packet.Type)
			{
				case MsgType.TypeSimImu:
				{
					MessageSimImu msg = default;
					msg.Decode(packet.Payload, packet.Length);
					_msgSimImuHandler(msg);
					break;
				}
				case MsgType.TypeSimAcc:
				{
					MessageSimAcc msg = default;
					msg.Decode(packet.Payload, packet.Length);

					break;
				}
				case MsgType.TypeSimGyro:
				{
					MessageSimGyro msg = default;
					msg.Decode(packet.Payload, packet.Length);

					break;
				}
				case MsgType.TypeSimMag:
					break;
				case MsgType.TypeSimBarometer:
					break;
			}
		}

		protected override void _msgSimImuHandler(MessageSimImu msg)
		{
			throw new System.NotImplementedException();
		}
	}
}
