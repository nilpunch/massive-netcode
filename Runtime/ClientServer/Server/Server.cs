using System;
using System.Collections.Generic;
using Massive.Serialization;

namespace Massive.Netcode
{
	public class Server
	{
		private int TicksAcceptWindow { get; }

		private ServerSerializer MessageSerializer { get; }

		public InputIdentifiers InputIdentifiers { get; }

		public WorldSerializer WorldSerializer { get; }

		public Session Session { get; }

		public List<Connection> Connections { get; } = new List<Connection>();

		public int LastSendedTick { get; set; }

		public Server(SessionConfig sessionConfig, double ticksAcceptWindowSeconds = 2f)
		{
			TicksAcceptWindow = (int)(sessionConfig.TickRate * ticksAcceptWindowSeconds);

			Session = new Session(sessionConfig, resimulate: false);

			InputIdentifiers = new InputIdentifiers();
			MessageSerializer = new ServerSerializer(Session.Inputs, InputIdentifiers);
			WorldSerializer = new WorldSerializer();
		}

		public void Update(double serverTime)
		{
			ReadMessages(serverTime);

			var targetTick = (int)Math.Floor(serverTime * Session.Config.TickRate);

			Session.Loop.FastForwardToTick(targetTick);

			if (LastSendedTick > targetTick)
			{
				return;
			}

			for (; LastSendedTick <= targetTick; LastSendedTick++)
			{
				foreach (var connection in Connections)
				{
					MessageSerializer.ServerWriteAllFresh(LastSendedTick, connection.Outgoing);
				}
			}

			foreach (var connection in Connections)
			{
				MessageSerializer.WriteMessageId((int)MessageType.Approve, connection.Outgoing);
				ApproveMessage.Write(new ApproveMessage() { Tick = targetTick }, connection.Outgoing);
			}
		}

		private void ReadMessages(double serverTime)
		{
			foreach (var connection in Connections)
			{
				connection.PopulateIncoming();

				while (!connection.IsBad && connection.HasUnreadPayload)
				{
					var messageId = MessageSerializer.ReadMessageId(connection.Incoming);

					if (!(messageId == (int)MessageType.Ping
						|| InputIdentifiers.IsRegistered(messageId)))
					{
						connection.IsBad = true;
						break;
					}

					var messageSize = MessageSerializer.GetMessageSize(messageId, connection.Incoming);
					if (connection.IncomingPayloadLength < messageSize)
					{
						MessageSerializer.UndoMessageIdRead(connection.Incoming);
						break;
					}

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

							MessageSerializer.WriteMessageId((int)MessageType.Pong, connection.Outgoing);
							PongMessage.Write(pongMessage, connection.Outgoing);
							break;
						}

						default:
						{
							var tick = connection.Incoming.ReadInt();
							if (CanAcceptTick(tick))
							{
								MessageSerializer.ServerReadOne(messageId, tick, connection.Channel, connection.Incoming);
							}
							else
							{
								MessageSerializer.ServerSkipOne(messageId, connection.Incoming);
							}
							break;
						}
					}
				}

				connection.CompactIncoming();
			}
		}

		public void SendFullSync(Connection connection)
		{
			MessageSerializer.WriteMessageId((int)MessageType.FullSync, connection.Outgoing);
			connection.Outgoing.WriteInt(Session.Loop.CurrentTick);
			WorldSerializer.Serialize(Session.World, connection.Outgoing);
			connection.Outgoing.WriteAllocator(Session.Systems.Allocator);
			MessageSerializer.ServerWriteMany(Session.Loop.CurrentTick, connection.Outgoing);
		}

		private bool CanAcceptTick(int tick)
		{
			return tick > Session.Loop.CurrentTick && tick < Session.Loop.CurrentTick + TicksAcceptWindow;
		}
	}
}
