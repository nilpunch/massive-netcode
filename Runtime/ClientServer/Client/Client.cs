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
			while (Incoming.CanRead)
			{
				var messageId = Incoming.Read1Byte();

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
						InputSerializer.ReadMany(Incoming);
						break;
					}

					default:
					{
						InputSerializer.Read(messageId, Incoming);
						break;
					}
				}
			}
		}

		public void OnInputPredicted(IInputSet inputSet, int tick, int channel)
		{
			inputSet.Write(tick, channel, Outgoing);
		}

		public void OnEventPredicted(IEventSet eventSet, int tick, int localOrder)
		{
			eventSet.Write(tick, localOrder, Outgoing);
		}
	}
}
