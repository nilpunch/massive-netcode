using Massive.Serialization;

namespace Massive.Netcode
{
	public class Client : IPredictionReceiver
	{
		private ClientSerializer MessageSerializer { get; }

		public InputIdentifiers InputIdentifiers { get; }

		public WorldSerializer WorldSerializer { get; }

		public Session Session { get; }

		public TickSync TickSync { get; }

		public Connection Connection { get; }

		private double PingDelaySeconds { get; }

		private double LastPingTime { get; set; } = -1;

		public Client(SessionConfig sessionConfig, Connection connection, double pingDelaySeconds = 1f)
		{
			Connection = connection;
			PingDelaySeconds = pingDelaySeconds;

			Session = new Session(sessionConfig);
			TickSync = new TickSync(sessionConfig.TickRate, sessionConfig.RollbackTicksCapacity);

			InputIdentifiers = new InputIdentifiers();
			MessageSerializer = new ClientSerializer(Session.Inputs, InputIdentifiers);
			WorldSerializer = new WorldSerializer();
		}

		public void Update(double clientTime)
		{
			if (!Connection.IsConnected)
			{
				return;
			}

			if (clientTime - LastPingTime >= 0.5f)
			{
				LastPingTime = clientTime;
				MessageSerializer.WriteMessageId((int)MessageType.Pong, Connection.Outgoing);
				PingMessage.Write(new PingMessage() { ClientPingSendTime = clientTime }, Connection.Outgoing);
			}

			ReadMessages(clientTime);

			Session.Loop.FastForwardToTick(TickSync.CalculateTargetTick(clientTime));
		}

		private void ReadMessages(double clientTime)
		{
			Connection.PopulateIncoming();

			while (Connection.IsConnected && Connection.HasUnreadPayload)
			{
				var messageId = MessageSerializer.ReadMessageId(Connection.Incoming);

				if (messageId == (int)MessageType.Ping
					|| !InputIdentifiers.IsRegistered(messageId))
				{
					Connection.Disconnect();
					break;
				}

				var messageSize = MessageSerializer.GetMessageSize(messageId, Connection.Incoming);
				if (Connection.IncomingPayloadLength < messageSize)
				{
					MessageSerializer.UndoMessageIdRead(Connection.Incoming);
					break;
				}

				switch (messageId)
				{
					case (int)MessageType.Pong:
					{
						var pongMessage = PongMessage.Read(Connection.Incoming);
						var rtt = clientTime - pongMessage.ClientPingSendTime;
						var serverTime = pongMessage.ServerReceiveTime + rtt * 0.5;
						TickSync.UpdateTimeSync(serverTime, clientTime);
						TickSync.UpdateRTT(rtt);
						break;
					}

					case (int)MessageType.FullSync:
					{
						Connection.Incoming.ReadInt(); // Payload length.

						var channel = Connection.Incoming.ReadInt();
						var serverTick = Connection.Incoming.ReadInt();
						Session.Reset(serverTick);
						WorldSerializer.Deserialize(Session.World, Connection.Incoming);
						Connection.Incoming.ReadAllocator(Session.Systems.Allocator);
						MessageSerializer.ReadMany(serverTick, Connection.Incoming);

						TickSync.Reset();
						TickSync.ApproveSimulationTick(serverTick);
						LastPingTime = -1;
						Connection.Channel = channel;
						break;
					}

					case (int)MessageType.Approve:
					{
						var approveMessage = ApproveMessage.Read(Connection.Incoming);
						TickSync.ApproveSimulationTick(approveMessage.ServerTick);
						break;
					}

					default:
					{
						var tick = Connection.Incoming.ReadInt();
						var channel = Connection.Incoming.ReadShort();
						MessageSerializer.ReadOne(messageId, tick, channel, Connection.Incoming);
						break;
					}
				}
			}

			Connection.CompactIncoming();
		}

		public void OnInputPredicted(IInputSet inputSet, int tick, int channel)
		{
			MessageSerializer.WriteOne(inputSet, tick, channel, Connection.Outgoing);
		}

		public void OnEventPredicted(IEventSet eventSet, int tick, int localOrder)
		{
			MessageSerializer.WriteOne(eventSet, tick, localOrder, Connection.Outgoing);
		}
	}
}
