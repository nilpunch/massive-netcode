using System.Collections.Generic;
using System.IO;
using Massive.Serialization;

namespace Massive.Netcode
{
	public struct Connection
	{
		public Stream Incoming;
		public Stream Outgoing;
	}

	public class Server
	{
		private InputSerializer InputSerializer { get; }

		public InputIdentifiers InputIdentifiers { get; }

		public WorldSerializer WorldSerializer { get; }

		public Session Session { get; }

		public TickSync TickSync { get; }

		public List<Connection> Connections { get; set; }

		public Server(SessionConfig sessionConfig)
		{
			Session = new Session(sessionConfig);
			TickSync = new TickSync(sessionConfig.TickRate, sessionConfig.RollbackTicksCapacity);

			InputIdentifiers = new InputIdentifiers((int)MessageType.Count);
			InputSerializer = new InputSerializer(Session.Inputs, InputIdentifiers);
			WorldSerializer = new WorldSerializer();
		}

		public void Update(double serverTime)
		{
			ReadMessages(serverTime);

			Session.Loop.FastForwardToTick(TickSync.CalculateTargetTick(serverTime));
		}

		private void ReadMessages(double serverTime)
		{
			foreach (var connection in Connections)
			{
				while (connection.Incoming.CanRead)
				{
					var messageId = SerializationUtils.ReadByte(connection.Incoming);

					switch (messageId)
					{
						case (int)MessageType.Ping:
						{
							var pingMessage = PingMessage.Read(connection.Incoming);
							var pongMessage = new PongMessage()
							{
								ClientPingSendTime = pingMessage.ClientPingSendTime,
								ServerReceiveTime = serverTime
							};

							SerializationUtils.WriteByte((int)MessageType.Pong, connection.Outgoing);
							PongMessage.Write(pongMessage, connection.Outgoing);
							break;
						}

						case (int)MessageType.FullSync:
						{
							SerializationUtils.WriteByte((int)MessageType.FullSync, connection.Outgoing);

							SerializationUtils.WriteInt(Session.Loop.CurrentTick, connection.Outgoing);
							WorldSerializer.Serialize(Session.World, connection.Outgoing);
							SerializationUtils.WriteAllocator(Session.Systems.Allocator, connection.Outgoing);
							InputSerializer.WriteFullSync(connection.Outgoing);
							break;
						}

						default:
						{
							// Need to add old input discaring.
							InputSerializer.ReadActualInput(messageId, connection.Incoming);
							break;
						}
					}
				}
			}
		}
	}
}
