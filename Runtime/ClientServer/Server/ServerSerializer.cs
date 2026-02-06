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
		public int GetMessageSize(int messageId)
		{
			switch (messageId)
			{
				case (int)MessageType.Ping:
				{
					return PingMessage.Size;
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

		public void ReadOneInput(int messageId, int tick, int channel, Stream stream)
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

		public void SkipOneInput(int messageId, Stream stream)
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

		public void WriteFullSyncInputs(int tick, Stream stream)
		{
			WriteMessageId(_inputs.EventSets.Count, stream);

			foreach (var eventSet in _inputs.EventSets)
			{
				var messageId = _inputIdentifiers.GetEventId(eventSet.EventType);
				WriteMessageId(messageId, stream);

				var eventsCount = eventSet.GetEventsCount(tick);
				stream.WriteShort((short)eventsCount);

				foreach (var localOrder in eventSet.GetEventsLocalOrders(tick))
				{
					var channel = eventSet.GetEventChannel(tick, localOrder);

					stream.WriteShort((short)localOrder);
					stream.WriteShort((short)channel);
					eventSet.WriteData(tick, localOrder, stream);
				}
			}

			WriteMessageId(_inputs.InputSets.Count, stream);

			foreach (var inputSet in _inputs.InputSets)
			{
				var messageId = _inputIdentifiers.GetInputId(inputSet.InputType);
				WriteMessageId(messageId, stream);

				var usedChannels = inputSet.GetUsedChannels(tick);
				stream.WriteShort((short)usedChannels);

				for (var channel = 0; channel < usedChannels; channel++)
				{
					inputSet.WriteInput(tick, channel, stream);
				}
			}
		}

		public void WriteAllFreshInputs(int tick, Stream stream)
		{
			foreach (var eventSet in _inputs.EventSets)
			{
				foreach (var localOrder in eventSet.GetEventsLocalOrders(tick))
				{
					WriteOneInput(eventSet, tick, localOrder, stream);
				}
			}

			foreach (var inputSet in _inputs.InputSets)
			{
				var usedChannels = inputSet.GetUsedChannels(tick);

				for (var channel = 0; channel < usedChannels; channel++)
				{
					if (inputSet.IsFresh(tick, channel))
					{
						WriteOneInput(inputSet, tick, channel, stream);
					}
				}
			}
		}

		public void WriteOneInput(IInputSet inputSet, int tick, int channel, Stream stream)
		{
			var messageId = _inputIdentifiers.GetInputId(inputSet.InputType);

			WriteMessageId(messageId, stream);
			stream.WriteInt(tick);
			stream.WriteShort((short)channel);
			inputSet.WriteData(tick, channel, stream);
		}

		public void WriteOneInput(IEventSet eventSet, int tick, int localOrder, Stream stream)
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
