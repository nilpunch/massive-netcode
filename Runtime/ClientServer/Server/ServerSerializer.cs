using System.IO;
using Massive.Serialization;

namespace Massive.Netcode
{
	public class ServerSerializer : MessageSerializer
	{
		public ServerSerializer(Inputs inputs, InputIdentifiers inputIdentifiers) : base(inputs, inputIdentifiers)
		{
		}

		/// <summary>
		/// Returns int.MaxValue if size not available.
		/// </summary>
		public int GetMessageSize(int messageId, Stream stream)
		{
			switch (messageId)
			{
				case (int)MessageType.Ping:
				{
					return 8;
				}

				case (int)MessageType.Pong:
				{
					return 16;
				}

				case (int)MessageType.FullSync:
				{
					var payloadLength = stream.Length - stream.Position;
					if (payloadLength < 4)
					{
						return int.MaxValue;
					}
					var messageSize = stream.ReadInt();
					stream.Position -= 4;
					return messageSize;
				}

				case (int)MessageType.Approve:
				{
					return 4;
				}

				default:
				{
					if (_inputIdentifiers.IsEvent(messageId))
					{
						return 4 + GetEventSet(messageId).EventDataSize;
					}
					else
					{
						return 4 + GetInputSet(messageId).DataSize;
					}
				}
			}
		}

		public void ServerReadOne(int messageId, int tick, int channel, Stream stream)
		{
			if (_inputIdentifiers.IsEvent(messageId))
			{
				var eventSet = GetEventSet(messageId);
				var localOrder = eventSet.AppendActualDefault(tick, channel);

				eventSet.ReadData(tick, localOrder, channel, stream);
			}
			else
			{
				GetInputSet(messageId).ReadData(tick, channel, stream);
			}
		}

		public void ServerSkipOne(int messageId, Stream stream)
		{
			if (_inputIdentifiers.IsEvent(messageId))
			{
				GetEventSet(messageId).SkipData(stream);
			}
			else
			{
				GetInputSet(messageId).SkipData(stream);
			}
		}

		public void ServerWriteMany(int tick, Stream stream)
		{
			_buffer.Position = 0;
			_buffer.SetLength(0);

			WriteMessageId(_inputs.EventSets.Count, _buffer);

			foreach (var eventSet in _inputs.EventSets)
			{
				var messageId = _inputIdentifiers.GetEventId(eventSet.EventType);
				WriteMessageId(messageId, _buffer);

				var eventsCount = eventSet.GetEventsCount(tick);
				_buffer.WriteShort((short)eventsCount);

				foreach (var localOrder in eventSet.GetEventsLocalOrders(tick))
				{
					var channel = eventSet.GetEventChannel(tick, localOrder);

					_buffer.WriteShort((short)localOrder);
					_buffer.WriteShort((short)channel);
					eventSet.WriteData(tick, localOrder, _buffer);
				}
			}

			WriteMessageId(_inputs.InputSets.Count, _buffer);

			foreach (var inputSet in _inputs.InputSets)
			{
				var messageId = _inputIdentifiers.GetInputId(inputSet.InputType);
				WriteMessageId(messageId, _buffer);

				var usedChannels = inputSet.GetUsedChannels(tick);
				_buffer.WriteShort((short)usedChannels);

				for (var channel = 0; channel < usedChannels; channel++)
				{
					inputSet.WriteInput(tick, channel, _buffer);
				}
			}

			stream.WriteInt((int)_buffer.Length);
			stream.Write(_buffer.GetBuffer(), 0, (int)_buffer.Length);
		}

		public void ServerWriteAllFresh(int tick, Stream stream)
		{
			foreach (var eventSet in _inputs.EventSets)
			{
				foreach (var localOrder in eventSet.GetEventsLocalOrders(tick))
				{
					ServerWriteOne(eventSet, tick, localOrder, stream);
				}
			}

			foreach (var inputSet in _inputs.InputSets)
			{
				var usedChannels = inputSet.GetUsedChannels(tick);

				for (var channel = 0; channel < usedChannels; channel++)
				{
					if (inputSet.IsFresh(tick, channel))
					{
						ServerWriteOne(inputSet, tick, channel, stream);
					}
				}
			}
		}

		public void ServerWriteOne(IInputSet inputSet, int tick, int channel, Stream stream)
		{
			var messageId = _inputIdentifiers.GetInputId(inputSet.InputType);

			WriteMessageId(messageId, stream);
			stream.WriteInt(tick);
			stream.WriteShort((short)channel);
			inputSet.WriteData(tick, channel, stream);
		}

		public void ServerWriteOne(IEventSet eventSet, int tick, int localOrder, Stream stream)
		{
			var messageId = _inputIdentifiers.GetEventId(eventSet.EventType);
			var channel = eventSet.GetEventChannel(tick, localOrder);

			WriteMessageId(messageId, stream);
			stream.WriteInt(tick);
			stream.WriteShort((short)channel);
			stream.WriteShort((short)localOrder);
			eventSet.WriteData(tick, localOrder, stream);
		}
	}
}
