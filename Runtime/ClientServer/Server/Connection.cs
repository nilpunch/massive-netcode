using System.Net.Sockets;

namespace Massive.Netcode
{
	public class Connection
	{
		public NetworkStream Incoming;
		public NetworkStream Outgoing;
		public int Channel;
		public bool IsBad;
	}
}
