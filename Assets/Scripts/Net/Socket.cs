using System.Net;
using System.Net.Sockets;
using System.Threading;
using Net.Protocol;
using UnityEngine;

namespace Net
{
	public delegate void ParseFunc(byte[] buffer, ushort length);

	public delegate ushort PacketFunc(IMessageSerializer msg, byte[] buffer, ushort maxLength);

	public class ServerSocket
	{
		private static readonly ServerSocket instance = new ServerSocket();
		private byte[] _buffer = new byte[1024];
		private TcpListener _listener;
		private TcpClient _client;
		private Thread _tcpListenerThread;
		public bool Connected = false;
		public ParseFunc ParseFunc;
		public PacketFunc PacketFunc;

		private ServerSocket()
		{
			// receive control
			_listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 14567);

			// send imu data
			_client = new TcpClient();
		}

		~ServerSocket()
		{
			_listener.Stop();
			_client.GetStream().Close();
			_client.Close();
		}

		public static ServerSocket Instance => instance;

		public void Start()
		{
			_tcpListenerThread = new Thread(ListenerForRemoteRequest) {IsBackground = true};
			_tcpListenerThread.Start();

			_client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 13456));
			if (!_client.Connected)
			{
				Debug.Log("Client connect failed");
			}
		}

		public void ReportImuData()
		{
		}

		private void ListenerForRemoteRequest()
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
							int length = 0;
							while (stream.CanRead && (length = stream.Read(_buffer, 0, _buffer.Length)) > 0)
							{
								ParseFunc(_buffer, (ushort) length);
							}
						}
					}
				}
			}
			catch (SocketException socketException)
			{
				Debug.Log("[SocketException] " + socketException.ToString());
			}
		}
	}
}
