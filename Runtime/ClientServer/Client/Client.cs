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

		private double PingIntervalSeconds { get; }

		private double LastPingTime { get; set; } = -1;

		public bool Synced { get; private set; }

		public Client(SessionConfig sessionConfig, Connection connection, double pingIntervalSeconds = 0.5f)
		{
			Connection = connection;
			PingIntervalSeconds = pingIntervalSeconds;

			Session = new Session(sessionConfig, this);
			TickSync = new TickSync(sessionConfig.TickRate, sessionConfig.RollbackTicksCapacity);

			InputIdentifiers = new InputIdentifiers();
			MessageSerializer = new ClientSerializer(Session.Inputs, InputIdentifiers);
			WorldSerializer = new WorldSerializer();
		}

		public int InputPredictionTick(double clientTime)
		{
			// +1 because we don't want to override approved inputs.
			return MathUtils.Max(TickSync.ApprovedSimulationTick + 1, TickSync.CalculateTargetTick(clientTime));
		}

		public void Update(double clientTime)
		{
			if (!Connection.IsConnected)
			{
				Synced = false;
				return;
			}

			ReadMessages(clientTime);

			if (Synced && clientTime - LastPingTime >= PingIntervalSeconds)
			{
				LastPingTime = clientTime;
				MessageSerializer.WriteMessageId(MessageType.Ping, Connection.Outgoing);
				PingMessage.Write(new PingMessage() { ClientPingSendTime = clientTime }, Connection.Outgoing);
			}

			Connection.PushOutgoing();

			if (!Synced)
			{
				return;
			}

			Session.Loop.FastForwardToTick(TickSync.CalculateTargetTick(clientTime));
		}

		private void ReadMessages(double clientTime)
		{
			Connection.PollIncoming();

			while (Connection.IsConnected && Connection.HasUnreadPayload)
			{
				var messageId = MessageSerializer.ReadMessageId(Connection.Incoming);

				if (messageId == (int)MessageType.Ping
					|| messageId >= (int)MessageType.Count && !InputIdentifiers.IsRegistered(messageId))
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
						Connection.Incoming.ReadInt(); // Skip payload length.

						Connection.Channel = Connection.Incoming.ReadInt();
						var serverTick = Connection.Incoming.ReadInt();
						Session.Reset(serverTick);
						WorldSerializer.Deserialize(Session.World, Connection.Incoming);
						Connection.Incoming.ReadAllocator(Session.Systems.Allocator);
						MessageSerializer.ReadFullSyncInputs(serverTick, Connection.Incoming);

						Session.World.SaveFrame();
						TickSync.Reset();
						TickSync.ApproveSimulationTick(serverTick);
						LastPingTime = -1;
						Synced = true;
						break;
					}

					case (int)MessageType.Approve:
					{
						var approveMessage = ApproveMessage.Read(Connection.Incoming);

						foreach (var inputSet in Session.Inputs.InputSets)
						{
							inputSet.ClearPrediction(TickSync.ApprovedSimulationTick, approveMessage.ServerTick);
						}
						foreach (var eventSet in Session.Inputs.EventSets)
						{
							eventSet.ClearPrediction(TickSync.ApprovedSimulationTick, approveMessage.ServerTick);
						}

						TickSync.ApproveSimulationTick(approveMessage.ServerTick);
						break;
					}

					default:
					{
						var tick = Connection.Incoming.ReadInt();
						var channel = Connection.Incoming.ReadShort();
						if (CanAcceptTick(tick))
						{
							MessageSerializer.ReadOneInput(messageId, tick, channel, Connection.Incoming);
						}
						else
						{
							MessageSerializer.SkipOneInput(messageId, Connection.Incoming);
						}
						break;
					}
				}
			}

			Connection.CompactIncoming();
		}

		void IPredictionReceiver.OnInputPredicted(IInputSet inputSet, int tick, int channel)
		{
			MessageSerializer.WriteOneInput(inputSet, tick, channel, Connection.Outgoing);
		}

		void IPredictionReceiver.OnEventPredicted(IEventSet eventSet, int tick, int order)
		{
			MessageSerializer.WriteOneInput(eventSet, tick, order, Connection.Outgoing);
		}

		private bool CanAcceptTick(int tick)
		{
			return tick > TickSync.ApprovedSimulationTick;
		}
	}
}
