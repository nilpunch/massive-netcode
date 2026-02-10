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
				var order = eventSet.AppendApprovedDefault(tick, channel);

				eventSet.ReadApproved(tick, order, channel, stream);
			}
			else
			{
				GetInputSet(messageId).ReadApproved(tick, channel, stream);
			}
		}

		public void SkipOneInput(int messageId, Stream stream)
		{
			if (_inputIdentifiers.IsEvent(messageId))
			{
				GetEventSet(messageId).Skip(stream);
			}
			else
			{
				GetInputSet(messageId).Skip(stream);
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

				foreach (var order in eventSet.GetAllEvents(tick))
				{
					var channel = eventSet.GetEventChannel(tick, order);

					stream.WriteShort((short)order);
					stream.WriteShort((short)channel);
					eventSet.Write(tick, order, stream);
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
					inputSet.WriteFullInput(tick, channel, stream);
				}
			}
		}

		public void WriteAllFreshInputs(int tick, Stream stream)
		{
			foreach (var eventSet in _inputs.EventSets)
			{
				foreach (var order in eventSet.GetAllEvents(tick))
				{
					WriteOneInput(eventSet, tick, order, stream);
				}
			}

			foreach (var inputSet in _inputs.InputSets)
			{
				foreach (var channel in inputSet.GetFreshInputs(tick))
				{
					WriteOneInput(inputSet, tick, channel, stream);
				}
			}
		}

		public void WriteOneInput(IInputSet inputSet, int tick, int channel, Stream stream)
		{
			var messageId = _inputIdentifiers.GetInputId(inputSet.InputType);

			WriteMessageId(messageId, stream);
			stream.WriteInt(tick);
			stream.WriteShort((short)channel);
			inputSet.Write(tick, channel, stream);
		}

		public void WriteOneInput(IEventSet eventSet, int tick, int order, Stream stream)
		{
			var messageId = _inputIdentifiers.GetEventId(eventSet.EventType);
			var channel = eventSet.GetEventChannel(tick, order);

			WriteMessageId(messageId, stream);
			stream.WriteInt(tick);
			stream.WriteShort((short)channel);
			stream.WriteShort((short)order);
			eventSet.Write(tick, order, stream);
		}
	}
}
