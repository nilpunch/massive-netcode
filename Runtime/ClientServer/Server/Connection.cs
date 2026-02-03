using System.IO;

namespace Massive.Netcode
{
	public struct Connection
	{
		public Stream Incoming;
		public Stream Outgoing;
		public int Channel;
	}
}
