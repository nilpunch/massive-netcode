using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Massive.Netcode
{
	public class TcpConnectionsListener : IConnectionListener
	{
		private static readonly Stack<TcpConnection> _pool = new Stack<TcpConnection>();

		private readonly TcpListener _listener;

		public TcpConnectionsListener(int port)
		{
			_listener = new TcpListener(IPAddress.Any, port);
		}

		public void Start() => _listener.Start();

		public void Stop() => _listener.Stop();

		public bool TryAccept(out Connection connection)
		{
			if (_listener.Pending())
			{
				var client = _listener.AcceptTcpClient();
				client.NoDelay = true;

				var tcpConnection = _pool.Count > 0 ? _pool.Pop() : new TcpConnection();
				tcpConnection.Init(client);

				connection = tcpConnection;
				return true;
			}

			connection = default;
			return false;
		}

		public void ReturnToPool(Connection connection)
		{
			if (connection is TcpConnection tcpConnection)
			{
				_pool.Push(tcpConnection);
			}
		}
	}
}
