using System.IO;
using Massive.Serialization;

namespace Massive.Netcode
{
	public struct ApproveMessage
	{
		public int Tick;

		public static ApproveMessage Read(Stream stream)
		{
			var tick = stream.ReadInt();
			return new ApproveMessage()
			{
				Tick = tick,
			};
		}

		public static void Write(ApproveMessage message, Stream stream)
		{
			stream.WriteInt(message.Tick);
		}
	}
}
