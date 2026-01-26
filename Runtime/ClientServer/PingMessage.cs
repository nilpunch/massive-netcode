using System.IO;
using Massive.Serialization;

namespace Massive.Netcode
{
	public struct PingMessage
	{
		public double ClientPingSendTime;

		public static PingMessage Read(Stream stream)
		{
			var clientSendTime = SerializationUtils.ReadDouble(stream);
			return new PingMessage()
			{
				ClientPingSendTime = clientSendTime,
			};
		}

		public static void Write(PingMessage message, Stream stream)
		{
			SerializationUtils.WriteDouble(message.ClientPingSendTime, stream);
		}
	}
}
