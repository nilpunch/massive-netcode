using Massive.Serialization;

namespace Massive.Netcode
{
	public class Client : IPredictionReceiver
	{
		private ClientSerializer MessageSerializer { get; }
		private ILogger Logger { get; }

		public InputIdentifiers InputIdentifiers { get; }

		public WorldSerializer WorldSerializer { get; }

		public Session Session { get; }

		public TickSync TickSync { get; }

		public Connection Connection { get; }

		private double PingIntervalSeconds { get; }

		private double LastPingTime { get; set; } = -1;

		public bool Synced { get; private set; }

		public Client(SessionConfig sessionConfig, Connection connection, double pingIntervalSeconds = 0.5f, TickSync tickSync = null, ILogger logger = null)
		{
			Connection = connection;
			PingIntervalSeconds = pingIntervalSeconds;

			Session = new Session(sessionConfig, this);
			TickSync = tickSync ?? new AdaptiveTickSync();
			TickSync.TickRate = sessionConfig.TickRate;
			TickSync.MaxRollbackTicks = sessionConfig.RollbackTicksCapacity;

			InputIdentifiers = new InputIdentifiers();
			MessageSerializer = new ClientSerializer(Session.Inputs, InputIdentifiers);
			WorldSerializer = new WorldSerializer();

			Logger = logger ?? NullLogger.Instance;

			Logger.Log($"Client initialized. TickRate={sessionConfig.TickRate}, PingInterval={pingIntervalSeconds}s");
		}

		public int InputPredictionTick(double clientTime)
		{
			return TickSync.CalculateTargetTick(clientTime);
		}

		public void Update(double clientTime)
		{
			if (!Connection.IsConnected)
			{
				if (Synced)
				{
					Logger.Warn("Connection lost. Marking client as de-synced.");
				}
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
					Logger.Error($"Received invalid message id={messageId} from server. Disconnecting.");
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
						Logger.Log($"Pong received. RTT={rtt * 1000:F1}ms, estimated server time={serverTime:F3}");
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
						
						Logger.Log($"Full sync received. Assigned channel={Connection.Channel}, serverTick={serverTick}");
						break;
					}

					case (int)MessageType.Approve:
					{
						var approveMessage = ApproveMessage.Read(Connection.Incoming);

						foreach (var inputSet in Session.Inputs.InputSets)
						{
							inputSet.ClearPrediction(TickSync.MinPredictionTick, approveMessage.ServerTick);
						}
						foreach (var eventSet in Session.Inputs.EventSets)
						{
							eventSet.ClearPrediction(TickSync.MinPredictionTick, approveMessage.ServerTick);
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
							Logger.Warn($"Dropped stale input from channel {channel} for tick {tick} (minPrediction={TickSync.MinPredictionTick})");
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
			return tick >= TickSync.MinPredictionTick;
		}
	}
}
