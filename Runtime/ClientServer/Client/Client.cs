using System.Net.Sockets;
using Massive.Serialization;

namespace Massive.Netcode
{
	public class Client : IPredictionReceiver
	{
		private InputSerializer InputSerializer { get; }

		public InputIdentifiers InputIdentifiers { get; }

		public WorldSerializer WorldSerializer { get; }

		public Session Session { get; }

		public TickSync TickSync { get; }

		public NetworkStream Incoming { get; private set; }

		public NetworkStream Outgoing { get; private set; }

		public int Channel { get; private set; }

		public Client(SessionConfig sessionConfig)
		{
			Session = new Session(sessionConfig);
			TickSync = new TickSync(sessionConfig.TickRate, sessionConfig.RollbackTicksCapacity);

			InputIdentifiers = new InputIdentifiers();
			InputSerializer = new InputSerializer(Session.Inputs, InputIdentifiers);
			WorldSerializer = new WorldSerializer();
		}

		public void Update(double clientTime)
		{
			ReadMessages(clientTime);

			Session.Loop.FastForwardToTick(TickSync.CalculateTargetTick(clientTime));
		}

		private void ReadMessages(double clientTime)
		{
			while (Incoming.DataAvailable)
			{
				var messageId = InputSerializer.ReadMessageId(Incoming);

				switch (messageId)
				{
					case (int)MessageType.Pong:
					{
						var pongMessage = PongMessage.Read(Incoming);
						var rtt = clientTime - pongMessage.ClientPingSendTime;
						var serverTime = pongMessage.ServerReceiveTime + rtt * 0.5;
						TickSync.UpdateTimeSync(serverTime, clientTime);
						TickSync.UpdateRTT(rtt);
						break;
					}

					case (int)MessageType.FullSync:
					{
						var serverTick = Incoming.ReadInt();
						Session.Reset(serverTick);
						WorldSerializer.Deserialize(Session.World, Incoming);
						Incoming.ReadAllocator(Session.Systems.Allocator);
						InputSerializer.ClientReadMany(Incoming);

						TickSync.Reset();
						TickSync.ApproveSimulationTick(serverTick);
						break;
					}

					case (int)MessageType.Approve:
					{
						var serverTick = Incoming.ReadInt();
						TickSync.ApproveSimulationTick(serverTick);
						break;
					}

					default:
					{
						var tick = Incoming.ReadInt();
						var channel = Incoming.ReadShort();
						InputSerializer.ClientReadOne(messageId, tick, channel, Incoming);
						break;
					}
				}
			}
		}

		public void OnInputPredicted(IInputSet inputSet, int tick, int channel)
		{
			InputSerializer.ClientWriteOne(inputSet, tick, channel, Outgoing);
		}

		public void OnEventPredicted(IEventSet eventSet, int tick, int localOrder)
		{
			InputSerializer.ClientWriteOne(eventSet, tick, localOrder, Outgoing);
		}
	}
}
