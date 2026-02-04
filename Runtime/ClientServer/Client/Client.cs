using System.Net.Sockets;
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

		public Connection Connection { get; private set; }

		public Client(SessionConfig sessionConfig)
		{
			Session = new Session(sessionConfig);
			TickSync = new TickSync(sessionConfig.TickRate, sessionConfig.RollbackTicksCapacity);

			InputIdentifiers = new InputIdentifiers();
			MessageSerializer = new ClientSerializer(Session.Inputs, InputIdentifiers);
			WorldSerializer = new WorldSerializer();
		}

		public void Update(double clientTime)
		{
			ReadMessages(clientTime);

			Session.Loop.FastForwardToTick(TickSync.CalculateTargetTick(clientTime));
		}

		private void ReadMessages(double clientTime)
		{
			Connection.PopulateIncoming();

			while (!Connection.IsBad && Connection.HasUnreadPayload)
			{
				var messageId = MessageSerializer.ReadMessageId(Connection.Incoming);

				if (messageId == (int)MessageType.Ping
					|| !InputIdentifiers.IsRegistered(messageId))
				{
					Connection.IsBad = true;
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
						var serverTick = Connection.Incoming.ReadInt();
						Session.Reset(serverTick);
						WorldSerializer.Deserialize(Session.World, Connection.Incoming);
						Connection.Incoming.ReadAllocator(Session.Systems.Allocator);
						MessageSerializer.ClientReadMany(serverTick, Connection.Incoming);

						TickSync.Reset();
						TickSync.ApproveSimulationTick(serverTick);
						break;
					}

					case (int)MessageType.Approve:
					{
						var serverTick = Connection.Incoming.ReadInt();
						TickSync.ApproveSimulationTick(serverTick);
						break;
					}

					default:
					{
						var tick = Connection.Incoming.ReadInt();
						var channel = Connection.Incoming.ReadShort();
						MessageSerializer.ClientReadOne(messageId, tick, channel, Connection.Incoming);
						break;
					}
				}
			}

			Connection.CompactIncoming();
		}

		public void OnInputPredicted(IInputSet inputSet, int tick, int channel)
		{
			MessageSerializer.ClientWriteOne(inputSet, tick, channel, Connection.Outgoing);
		}

		public void OnEventPredicted(IEventSet eventSet, int tick, int localOrder)
		{
			MessageSerializer.ClientWriteOne(eventSet, tick, localOrder, Connection.Outgoing);
		}
	}
}
