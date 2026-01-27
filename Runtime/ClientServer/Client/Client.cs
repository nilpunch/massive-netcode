using System;
using System.IO;
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

		public Stream Incoming { get; private set; }

		public Stream Outgoing { get; private set; }

		public bool IsConnected { get; private set; }

		public Client(SessionConfig sessionConfig)
		{
			Session = new Session(sessionConfig);
			TickSync = new TickSync(sessionConfig.TickRate, sessionConfig.RollbackTicksCapacity);

			InputIdentifiers = new InputIdentifiers((int)MessageType.Count);
			InputSerializer = new InputSerializer(Session.Inputs, InputIdentifiers);
			WorldSerializer = new WorldSerializer();
		}

		public void Update(double clientTime)
		{
			if (IsConnected)
			{
				ReadMessages(clientTime);
			}

			Session.Loop.FastForwardToTick(TickSync.CalculateTargetTick(clientTime));
		}

		private void ReadMessages(double clientTime)
		{
			while (Incoming.CanRead)
			{
				var messageId = SerializationUtils.ReadByte(Incoming);

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
						var serverTick = SerializationUtils.ReadInt(Incoming);
						Session.Reset(serverTick);

						WorldSerializer.Deserialize(Session.World, Incoming);
						SerializationUtils.ReadAllocator(Session.Systems.Allocator, Incoming);
						InputSerializer.ReadFullSync(Incoming);
						break;
					}

					default:
					{
						if (!InputIdentifiers.IsRegistered(messageId))
						{
							throw new InvalidOperationException();
						}

						InputSerializer.ReadActualInput(messageId, Incoming);
						break;
					}
				}
			}
		}

		void IPredictionReceiver.OnInputPredicted<T>(int tick, int channel)
		{
			Session.Inputs.GetInputSetSerializer<T>().WriteOne(tick, channel, Outgoing);
		}

		void IPredictionReceiver.OnEventPredicted<T>(int tick, int localOrder)
		{
			Session.Inputs.GetEventSetSerializer<T>().WriteOne(tick, localOrder, Outgoing);
		}
	}
}
