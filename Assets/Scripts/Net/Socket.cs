using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using Net.Protocol;
using UnityEngine;

namespace Net
{
	public delegate void ParseFunc(NetworkStream stream, byte[] buffer, ushort length);

	public delegate ushort PacketFunc(MessageSerializer msg, byte[] buffer, ushort maxLength);

	public class ServerSocket
	{
		private byte[] _buffer = new byte[1024];
		private TcpListener _listener;
		private Thread _tcpListenerThread;
		private Thread _respThread;
		public bool Connected = false;
		public ParseFunc ParseFunc;
		public PacketFunc PacketFunc;
		public Channel<MessageSerializer> Resp;

		private ServerSocket()
		{
			// receive control
			_listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 14567);
			Resp = Channel.CreateUnbounded<MessageSerializer>();
		}

		~ServerSocket()
		{
			_listener.Stop();
		}

		public static ServerSocket Instance { get; } = new ServerSocket();

		public void Start()
		{
			_tcpListenerThread = new Thread(_listenerForRemoteRequest) {IsBackground = true};
			_tcpListenerThread.Start();
			_respThread = new Thread(_processResponse) {IsBackground = true};
			_respThread.Start();

		}

		private void Response(MessageSerializer msg)
		{
			if (!msg.Stream.CanWrite)
			{
				return;
			}
			var buffer = new byte[255];
			var length = PacketFunc(msg, buffer, 255);

			msg.Stream.Write(buffer, 0, length);
		}

		private void _listenerForRemoteRequest()
		{
			try
			{
				_listener.Start();
				Debug.Log("Server started.");

				while (true)
				{
					using (var tcpClient = _listener.AcceptTcpClient())
					{
						using (var stream = tcpClient.GetStream())
						{
							var reader = new BinaryReader(stream);
							while (true)
							{
								var length = reader.Read(_buffer, 0, _buffer.Length);
								if (length > 0)
								{
									ParseFunc(stream, _buffer, (ushort) length);
								}
							}
						}
					}
				}
			}
			catch (SocketException socketException)
			{
				Debug.Log("[SocketException] " + socketException);
			}
		}

		private async void _processResponse()
		{
			while (await Resp.Reader.WaitToReadAsync())
			{
				if (Resp.Reader.TryRead(out var msg))
				{
					Response(msg);
				}
			}
		}
	}
}
