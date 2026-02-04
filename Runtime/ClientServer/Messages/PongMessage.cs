using System.IO;
using Massive.Serialization;

namespace Massive.Netcode
{
	public struct PongMessage
	{
		public static readonly int Size = ReflectionUtils.SizeOfUnmanaged(typeof(PongMessage));

		public double ClientPingSendTime;
		public double ServerReceiveTime;

		public static PongMessage Read(Stream stream)
		{
			var clientSendTime = stream.ReadDouble();
			var serverReceiveTime = stream.ReadDouble();
			return new PongMessage()
			{
				ClientPingSendTime = clientSendTime,
				ServerReceiveTime = serverReceiveTime,
			};
		}

		public static void Write(PongMessage message, Stream stream)
		{
			stream.WriteDouble(message.ClientPingSendTime);
			stream.WriteDouble(message.ServerReceiveTime);
		}
	}
}
