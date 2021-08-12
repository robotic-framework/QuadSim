using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Net.Protocol
{
	public enum MspType : byte
	{
		MspSimImu = 30,
		MspSimControl = 31,
		MspSimCommand = 32
	}

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

		private static readonly Dictionary<MsgType, MspType> _msgTypeMapping =
			new Dictionary<MsgType, MspType>
			{
				{MsgType.TypeSimImu, MspType.MspSimImu},
				{MsgType.TypeSimControl, MspType.MspSimControl},
				{MsgType.TypeSimCommand, MspType.MspSimCommand},
			};

		private static readonly Dictionary<MspType, MsgType> _mspTypeMapping =
			new Dictionary<MspType, MsgType>
			{
				{MspType.MspSimImu, MsgType.TypeSimImu},
				{MspType.MspSimControl, MsgType.TypeSimControl},
				{MspType.MspSimCommand, MsgType.TypeSimCommand},
			};

		public ProtocolMsp()
		{
			_handler = new CommonHandler();
		}

		public static byte MsgTypeMapping(MsgType t)
		{
			return (byte) _msgTypeMapping[t];
		}

		public static MsgType MspTypeMapping(MspType t)
		{
			return _mspTypeMapping[t];
		}

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
					// expect '>'
					_state = (c == '>') ? MspProtocolState.MspHeaderArrow : MspProtocolState.MspIdle;
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
						_state = MspProtocolState.MspHeaderSize;
					}

					break;
				}
				case MspProtocolState.MspHeaderSize:
				{
					// expect cmd
					_checksum ^= c;
					_dataOffset = 0;
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
						_state = MspProtocolState.MspIdle;

						// the last byte is checksum, check if equal
						if (_checksum == c)
						{
							return true;
						}
					}

					break;
				}
			}

			return false;
		}

		protected override MessageSerializer _processPacket(byte[] buffer, ushort length)
		{
			if (length < 6)
			{
				return null;
			}

			MsgPacket packet;
			packet.Payload = new byte[255];

			// copy payload
			if (length > 6)
			{
				Array.Copy(buffer, 5, packet.Payload, 0, length - 6);
			}

			packet.Length = buffer[3];
			packet.Type = MspTypeMapping((MspType) buffer[4]);

			MessageSerializer msg = null;
			switch (packet.Type)
			{
				case MsgType.TypeSimImu:
				{
					msg = _handler.msgSimImuHandler();
					break;
				}
				case MsgType.TypeSimControl:
				{
					var request = new MessageRequestControl();
					request.Decode(packet.Payload, packet.Length);
					_handler.msgSimControlHandler(request);
					break;
				}
				case MsgType.TypeSimCommand:
				{
					msg = _handler.msgSimCommandHandler();
					break;
				}
			}

			return msg;
		}

		protected override ushort _encode(MessageSerializer msg, byte[] buffer, ushort maxLength)
		{
			ushort offset = 0;
			var header = "$M<";
			var payload = new byte[255];
			var payloadLength = msg.Encode(payload, 255);
			var command = MsgTypeMapping(msg.type);

			Array.Copy(Encoding.ASCII.GetBytes(header), 0, buffer, offset, header.Length);
			offset += (ushort) header.Length;
			buffer.SetValue(payloadLength, offset);
			offset++;
			buffer.SetValue(command, offset);
			offset++;
			Array.Copy(payload, 0, buffer, offset, payloadLength);
			offset += payloadLength;

			var checksum = buffer[3];
			for (int i = 4; i < offset; i++)
			{
				checksum ^= buffer[i];
			}

			buffer.SetValue(checksum, offset);
			offset++;

			return offset;
		}
	}
}
