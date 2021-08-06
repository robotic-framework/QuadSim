using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Net.Protocol
{
	public enum MspType : byte
	{
		MspSimImu = 30,
		MspSimAcc = 31,
		MspSimGyro = 32,
		MspSimMag = 33,
		MspSimBaro = 34,
		MspSimControl = 35,
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
				{MsgType.TypeSimAcc, MspType.MspSimAcc},
				{MsgType.TypeSimGyro, MspType.MspSimGyro},
				{MsgType.TypeSimMag, MspType.MspSimMag},
				{MsgType.TypeSimBarometer, MspType.MspSimBaro},
				{MsgType.TypeSimControl, MspType.MspSimControl},
			};

		private static readonly Dictionary<MspType, MsgType> _mspTypeMapping =
			new Dictionary<MspType, MsgType>
			{
				{MspType.MspSimImu, MsgType.TypeSimImu},
				{MspType.MspSimAcc, MsgType.TypeSimAcc},
				{MspType.MspSimGyro, MsgType.TypeSimGyro},
				{MspType.MspSimMag, MsgType.TypeSimMag},
				{MspType.MspSimBaro, MsgType.TypeSimBarometer},
				{MspType.MspSimControl, MsgType.TypeSimControl},
			};

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

			Debug.Log(packet.Type.ToString());

			MessageSerializer msg = null;
			switch (packet.Type)
			{
				case MsgType.TypeSimImu:
				{
					var request = new MessageRequestSimImu();
					request.Decode(packet.Payload, packet.Length);
					msg = _msgSimImuHandler(request);
					break;
				}
				case MsgType.TypeSimAcc:
				{
					break;
				}
				case MsgType.TypeSimGyro:
				{
					break;
				}
				case MsgType.TypeSimMag:
					break;
				case MsgType.TypeSimBarometer:
					break;
				case MsgType.TypeSimControl:
				{
					var request = new MessageRequestControl();
					request.Decode(packet.Payload, packet.Length);
					_msgSimControlHandler(request);
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

		protected override MessageResponseSimImu _msgSimImuHandler(MessageRequestSimImu request)
		{
			var msg = new MessageResponseSimImu
			{
				Acc = {X = 1, Y = 2, Z = 3},
				Gyro = {X = 4, Y = 5, Z = 6},
				Mag = {X = 7, Y = 8, Z = 9},
				Ct = 1000,
				Cp = 10000,
				Ccp = 20000,
				Att = {X = 0, Y = 0, Z = 0},
				Alt = 50,
				Vario = 0
			};

			return msg;
		}

		protected override void _msgSimControlHandler(MessageRequestControl request)
		{
			MotorController.instance.motors = request.Motors;
		}
	}
}
