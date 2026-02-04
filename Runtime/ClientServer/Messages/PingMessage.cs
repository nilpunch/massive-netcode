using System.IO;
using Massive.Serialization;

namespace Massive.Netcode
{
	public struct PingMessage
	{
		public static readonly int Size = ReflectionUtils.SizeOfUnmanaged(typeof(PingMessage));

		public double ClientPingSendTime;

		public static PingMessage Read(Stream stream)
		{
			var clientSendTime = stream.ReadDouble();
			return new PingMessage()
			{
				ClientPingSendTime = clientSendTime,
			};
		}

		public static void Write(PingMessage message, Stream stream)
		{
			stream.WriteDouble(message.ClientPingSendTime);
		}
	}
}
