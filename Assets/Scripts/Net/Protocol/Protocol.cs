using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace Net.Protocol
{
	public enum MsgType
	{
		TypeSimImu,
		TypeSimControl,
		TypeSimCommand
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
		protected IHandler _handler;

		public async void ReceiveStream(NetworkStream stream, byte[] buf, ushort length)
		{
			for (ushort i = 0; i < length; i++)
			{
				_buffer.SetValue(buf[i], _bufferIndex);
				_bufferLength++;

				if (_decode(buf[i]))
				{
					var msg = _processPacket(_buffer, _bufferLength);
					if (msg != null)
					{
						msg.WithStream(stream);
						await ServerSocket.Instance.Resp.Writer.WriteAsync(msg);
					}
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

		public ushort PacketStream(MessageSerializer msg, byte[] buffer, ushort maxLength)
		{
			return _encode(msg, buffer, maxLength);
		}

		protected abstract bool _decode(byte c);
		protected abstract MessageSerializer _processPacket(byte[] buffer, ushort length);
		protected abstract ushort _encode(MessageSerializer msg, byte[] buffer, ushort maxLength);
	}
}
