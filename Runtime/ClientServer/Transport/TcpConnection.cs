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
			Outgoing = tcpClient.GetStream();
			Incoming.SetLength(0);
			Incoming.Position = 0;
		}

		public override void Connect(IPEndPoint endPoint)
		{
			if (TcpClient != null && TcpClient.Connected)
			{
				return;
			}

			TcpClient?.Close();

			TcpClient = new TcpClient();
			TcpClient.NoDelay = true;
			TcpClient.Connect(endPoint);
		}

		public override void Disconnect()
		{
			TcpClient?.Close();
		}
	}
}
