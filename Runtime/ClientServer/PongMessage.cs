using System.IO;
using Massive.Serialization;

namespace Massive.Netcode
{
	public struct PongMessage
	{
		public double ClientPingSendTime;
		public double ServerReceiveTime;

		public static PongMessage Read(Stream stream)
		{
			var clientSendTime = SerializationUtils.ReadDouble(stream);
			var serverReceiveTime = SerializationUtils.ReadDouble(stream);
			return new PongMessage()
			{
				ClientPingSendTime = clientSendTime,
				ServerReceiveTime = serverReceiveTime,
			};
		}

		public static void Write(PongMessage message, Stream stream)
		{
			SerializationUtils.WriteDouble(message.ClientPingSendTime, stream);
			SerializationUtils.WriteDouble(message.ServerReceiveTime, stream);
		}
	}
}
