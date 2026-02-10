using System;
using System.Collections.Generic;
using System.IO;
using Massive.Serialization;

namespace Massive.Netcode
{
	public class Server
	{
		private const int MaxMessagesPerClient = 50;

		private MemoryStream Buffer { get; } = new MemoryStream();

		private int TicksAcceptWindow { get; }

		private ServerSerializer MessageSerializer { get; }

		public InputIdentifiers InputIdentifiers { get; }

		public WorldSerializer WorldSerializer { get; }

		public Session Session { get; }

		public IConnectionListener ConnectionListener { get; }

		public List<Connection> Connections { get; } = new List<Connection>();
		public List<Connection> NewConnections { get; } = new List<Connection>();
		public int UsedChannels { get; private set; }

		public Server(SessionConfig sessionConfig, IConnectionListener connectionListener, double ticksAcceptWindowSeconds = 2f)
		{
			ConnectionListener = connectionListener;

			TicksAcceptWindow = (int)(sessionConfig.TickRate * ticksAcceptWindowSeconds);

			Session = new Session(sessionConfig, resimulate: false);

			InputIdentifiers = new InputIdentifiers();
			MessageSerializer = new ServerSerializer(Session.Inputs, InputIdentifiers);
			WorldSerializer = new WorldSerializer();
		}

		public void Update(double serverTime)
		{
			Session.Inputs.PopulateUpTo(Session.Loop.CurrentTick);

			for (var i = Connections.Count - 1; i >= 0; i--)
			{
				var connection = Connections[i];
				if (!connection.IsConnected)
				{
					Connections.RemoveAt(i);
					ConnectionListener.ReturnToPool(connection);
					Session.Inputs.AppendApprovedEventAt(Session.Loop.CurrentTick, connection.Channel, new PlayerDisconnectedEvent());
				}
			}

			while (ConnectionListener.TryAccept(out var connection))
			{
				connection.Channel = UsedChannels++;
				Connections.Add(connection);
				Session.Inputs.AppendApprovedEventAt(Session.Loop.CurrentTick, connection.Channel, new PlayerConnectedEvent());
				NewConnections.Add(connection);
			}

			ReadMessages(serverTime);

			var lastTick = Session.Loop.CurrentTick;
			var targetTick = (int)Math.Floor(serverTime * Session.Config.TickRate);

			var simulatingNewTick = targetTick > lastTick;
			if (simulatingNewTick)
			{
				foreach (var newConnection in NewConnections)
				{
					if (newConnection.IsConnected)
					{
						SendFullSync(newConnection);
					}
				}
				NewConnections.Clear();
			}

			Session.Loop.FastForwardToTick(targetTick);

			if (simulatingNewTick)
			{
				for (var tick = lastTick; tick < targetTick; tick++)
				{
					foreach (var connection in Connections)
					{
						MessageSerializer.WriteAllFreshInputs(tick, connection.Outgoing);
					}
				}

				Session.Inputs.DiscardUpTo(targetTick);

				// Approve previous tick.
				foreach (var connection in Connections)
				{
					MessageSerializer.WriteMessageId(MessageType.Approve, connection.Outgoing);
					ApproveMessage.Write(new ApproveMessage() { ServerTick = targetTick - 1 }, connection.Outgoing);
				}
			}

			foreach (var connection in Connections)
			{
				connection.PushOutgoing();
			}
		}

		private void ReadMessages(double serverTime)
		{
			foreach (var connection in Connections)
			{
				connection.PollIncoming();

				var messagesRead = 0;
				while (connection.IsConnected && connection.HasUnreadPayload && messagesRead < MaxMessagesPerClient)
				{
					var messageId = MessageSerializer.ReadMessageId(connection.Incoming);

					if (!IsAppropriateClientMessage(messageId))
					{
						connection.Disconnect();
						break;
					}

					var messageSize = MessageSerializer.GetMessageSize(messageId);
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

							MessageSerializer.WriteMessageId(MessageType.Pong, connection.Outgoing);
							PongMessage.Write(pongMessage, connection.Outgoing);
							break;
						}

						default:
						{
							var tick = connection.Incoming.ReadInt();
							if (CanAcceptTick(tick))
							{
								MessageSerializer.ReadOneInput(messageId, tick, connection.Channel, connection.Incoming);
							}
							else
							{
								MessageSerializer.SkipOneInput(messageId, connection.Incoming);
							}
							break;
						}
					}

					messagesRead += 1;
				}

				connection.CompactIncoming();

				if (messagesRead >= MaxMessagesPerClient)
				{
					connection.Disconnect();
				}
			}
		}

		private bool IsAppropriateClientMessage(int messageId)
		{
			if (messageId == (int)MessageType.Ping)
			{
				return true;
			}

			if (InputIdentifiers.IsRegistered(messageId) && !InputIdentifiers.IsAuthoritive(messageId))
			{
				return true;
			}

			return false;
		}

		private void SendFullSync(Connection connection)
		{
			Buffer.WriteInt(connection.Channel);
			Buffer.WriteInt(Session.Loop.CurrentTick);
			WorldSerializer.Serialize(Session.World, Buffer);
			Buffer.WriteAllocator(Session.Systems.Allocator);
			MessageSerializer.WriteFullSyncInputs(Session.Loop.CurrentTick, Buffer);

			MessageSerializer.WriteMessageId(MessageType.FullSync, connection.Outgoing);
			connection.Outgoing.WriteInt((int)Buffer.Length);
			connection.Outgoing.Write(Buffer.GetBuffer(), 0, (int)Buffer.Length);

			Buffer.Position = 0;
			Buffer.SetLength(0);
		}

		private bool CanAcceptTick(int tick)
		{
			return tick >= Session.Loop.CurrentTick && tick <= Session.Loop.CurrentTick + TicksAcceptWindow;
		}
	}
}
