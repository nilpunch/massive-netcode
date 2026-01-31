using System;
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

		public List<Connection> Connections { get; set; }

		public Server(SessionConfig sessionConfig)
		{
			Session = new Session(sessionConfig);

			InputIdentifiers = new InputIdentifiers();
			InputSerializer = new InputSerializer(Session.Inputs, InputIdentifiers, AcceptFutureInputs);
			WorldSerializer = new WorldSerializer();

			bool AcceptFutureInputs(int tick)
			{
				return tick > Session.Loop.CurrentTick;
			}
		}

		public void Update(double serverTime)
		{
			ReadMessages(serverTime);

			var targetTick = (int)Math.Floor(serverTime * Session.Config.TickRate);

			Session.Loop.FastForwardToTick(targetTick);
		}

		private void ReadMessages(double serverTime)
		{
			foreach (var connection in Connections)
			{
				while (connection.Incoming.CanRead)
				{
					var messageId = connection.Incoming.Read1Byte();

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

							connection.Outgoing.Write1Byte((int)MessageType.Pong);
							PongMessage.Write(pongMessage, connection.Outgoing);
							break;
						}

						case (int)MessageType.FullSync:
						{
							connection.Outgoing.Write1Byte((int)MessageType.FullSync);

							connection.Outgoing.WriteInt(Session.Loop.CurrentTick);
							WorldSerializer.Serialize(Session.World, connection.Outgoing);
							connection.Outgoing.WriteAllocator(Session.Systems.Allocator);
							InputSerializer.WriteMany(Session.Loop.CurrentTick, connection.Outgoing);
							break;
						}

						default:
						{
							// Need to add old input discaring.
							InputSerializer.Read(messageId, connection.Incoming);
							break;
						}
					}
				}
			}
		}
	}
}
