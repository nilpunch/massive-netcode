using System.Net;
using System.Net.Sockets;

namespace Massive.Netcode
{
	public class TcpConnection : Connection
	{
		public TcpClient TcpClient { get; set; }

		public override bool IsConnected => TcpClient != null && TcpClient.Connected;

		public void Init(TcpClient tcpClient)
		{
			TcpClient = tcpClient;
			Stream = tcpClient.GetStream();
			Incoming.SetLength(0);
			Incoming.Position = 0;
			Outgoing.SetLength(0);
			Outgoing.Position = 0;
		}

		public override void Connect(IPEndPoint endPoint)
		{
			TcpClient?.Close();
			TcpClient?.Dispose();

			TcpClient = new TcpClient();
			TcpClient.NoDelay = true;
			TcpClient.Connect(endPoint);

			Stream = TcpClient.GetStream();

			Incoming.SetLength(0);
			Incoming.Position = 0;

			Outgoing.SetLength(0);
			Outgoing.Position = 0;
		}

		public override void Disconnect()
		{
			TcpClient?.Close();
			TcpClient?.Dispose();
			TcpClient = null;
		}
	}
}
