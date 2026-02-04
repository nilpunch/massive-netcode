using System.IO;
using Massive.Serialization;

namespace Massive.Netcode
{
	public struct ApproveMessage
	{
		public static readonly int Size = ReflectionUtils.SizeOfUnmanaged(typeof(ApproveMessage));

		public int ServerTick;

		public static ApproveMessage Read(Stream stream)
		{
			var tick = stream.ReadInt();
			return new ApproveMessage()
			{
				ServerTick = tick,
			};
		}

		public static void Write(ApproveMessage message, Stream stream)
		{
			stream.WriteInt(message.ServerTick);
		}
	}
}
