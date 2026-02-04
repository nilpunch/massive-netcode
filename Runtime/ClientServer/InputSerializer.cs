using System.IO;
using Massive.Serialization;

namespace Massive.Netcode
{
	public partial class InputSerializer
	{
		private readonly InputIdentifiers _inputIdentifiers;

		private readonly Inputs _inputs;

		public InputSerializer(Inputs inputs, InputIdentifiers inputIdentifiers)
		{
			_inputIdentifiers = inputIdentifiers;
			_inputs = inputs;
		}

		public void WriteMessageId(int messageId, Stream stream)
		{
			stream.Write1Byte((byte)messageId);
		}

		public int ReadMessageId(Stream stream)
		{
			return stream.Read1Byte();
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

		public void ClientReadOne(int messageId, int tick, int channel, Stream stream)
		{
			if (_inputIdentifiers.IsEvent(messageId))
			{
				var localOrder = stream.ReadShort();
				GetEventSet(messageId).ReadData(tick, localOrder, channel, stream);
			}
			else
			{
				GetInputSet(messageId).ReadData(tick, channel, stream);
			}
		}

		public void ClientReadMany(Stream stream)
		{
			var tick = stream.ReadInt();
			var eventSetsCount = ReadMessageId(stream);

			for (var i = 0; i < eventSetsCount; i++)
			{
				var messageId = ReadMessageId(stream);
				var eventsCount = stream.ReadShort();

				var eventSet = GetEventSet(messageId);

				for (var j = 0; j < eventsCount; j++)
				{
					var localOrder = stream.ReadShort();
					var channel = stream.ReadShort();
					eventSet.ReadData(tick, localOrder, channel, stream);
				}
			}

			var inputSetsCount = ReadMessageId(stream);

			for (var i = 0; i < inputSetsCount; i++)
			{
				var messageId = ReadMessageId(stream);
				var usedChannels = stream.ReadShort();

				var inputsSet = GetInputSet(messageId);

				for (var channel = 0; channel < usedChannels; channel++)
				{
					inputsSet.ReadInput(tick, channel, stream);
				}
			}
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
		
		public void ServerWriteMany(int tick, Stream stream)
		{
			stream.WriteInt(tick);
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

		public void ClientWriteOne(IEventSet eventSet, int tick, int localOrder, Stream stream)
		{
			var messageId = _inputIdentifiers.GetEventId(eventSet.EventType);

			WriteMessageId(messageId, stream);
			stream.WriteInt(tick);
			eventSet.WriteData(tick, localOrder, stream);

			// Don't writing channel because server already know it.
			// Don't writing localOrder because server will append this event and use its own ordering.
		}

		public void ClientWriteOne(IInputSet inputSet, int tick, int channel, Stream stream)
		{
			var messageId = _inputIdentifiers.GetInputId(inputSet.InputType);

			WriteMessageId(messageId, stream);
			stream.WriteInt(tick);
			inputSet.WriteData(tick, channel, stream);

			// Don't writing channel because server already know it.
		}
	}
}
