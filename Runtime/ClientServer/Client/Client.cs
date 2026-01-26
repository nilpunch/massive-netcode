using System;
using System.IO;
using Massive.Serialization;

namespace Massive.Netcode
{
	public interface IConnection
	{
		public Stream Incoming { get; }
		public Stream Outgoing { get; }

		public bool IsConnected { get; }
	}

	public class Client
	{
		public InputIdentifiers InputIdentifiers { get; }

		private InputSerializer InputSerializer { get; }

		public WorldSerializer WorldSerializer { get; }

		public Session Session { get; }

		public TickSync TickSync { get; }

		public IConnection Connection { get; private set; }

		public Client(SessionConfig sessionConfig)
		{
			Session = new Session(sessionConfig);
			TickSync = new TickSync(sessionConfig.TickRate, sessionConfig.RollbackTicksCapacity);

			InputIdentifiers = new InputIdentifiers((int)MessageType.Count);
			InputSerializer = new InputSerializer(Session.Inputs, InputIdentifiers);
			WorldSerializer = new WorldSerializer();
		}

		public void Connect(IConnection connection)
		{
			Connection = connection;
		}

		public void Update(double clientTime)
		{
			if (Connection.IsConnected)
			{
				ReadMessages(clientTime);
			}

			Session.Loop.FastForwardToTick(TickSync.CalculateTargetTick(clientTime));
		}

		private void ReadMessages(double clientTime)
		{
			while (Connection.Incoming.CanRead)
			{
				var messageTypeId = SerializationUtils.ReadByte(Connection.Incoming);

				switch (messageTypeId)
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
						var serverTick = SerializationUtils.ReadInt(Connection.Incoming);
						Session.Reset(serverTick);

						WorldSerializer.Deserialize(Session.World, Connection.Incoming);
						SerializationUtils.ReadAllocator(Session.Systems.Allocator, Connection.Incoming);
						InputSerializer.ReadActualAndPredictionInputs(Connection.Incoming);
						break;
					}

					default:
					{
						InputSerializer.ReadActualInput(messageTypeId, Connection.Incoming);
						break;
					}
				}
			}
		}
	}
}
