using System.IO;
using Massive.Serialization;

namespace Massive.Netcode
{
	public class ClientSerializer : MessageSerializer
	{
		public ClientSerializer(Inputs inputs, InputIdentifiers inputIdentifiers) : base(inputs, inputIdentifiers)
		{
		}

		/// <summary>
		/// Returns int.MaxValue if size not available.
		/// </summary>
		public int GetMessageSize(int messageId, Stream stream)
		{
			switch (messageId)
			{
				case (int)MessageType.Pong:
				{
					return PongMessage.Size;
				}

				case (int)MessageType.FullSync:
				{
					var payloadLength = stream.Length - stream.Position;
					if (payloadLength < sizeof(int))
					{
						return int.MaxValue;
					}
					var messageSize = stream.ReadInt();
					stream.Position -= sizeof(int);
					return messageSize + sizeof(int);
				}

				case (int)MessageType.Approve:
				{
					return ApproveMessage.Size;
				}

				default:
				{
					if (_inputIdentifiers.IsEvent(messageId))
					{
						return 4 + 2 + 2 + GetEventSet(messageId).EventDataSize;
					}
					else
					{
						return 4 + 2 + GetInputSet(messageId).DataSize;
					}
				}
			}
		}

		public void ReadOneInput(int messageId, int tick, int channel, Stream stream)
		{
			if (_inputIdentifiers.IsEvent(messageId))
			{
				var order = stream.ReadShort();
				GetEventSet(messageId).ReadApproved(tick, order, channel, stream);
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
				stream.ReadShort();
				GetEventSet(messageId).Skip(stream);
			}
			else
			{
				GetInputSet(messageId).Skip(stream);
			}
		}

		public void ReadFullSyncInputs(int tick, Stream stream)
		{
			var eventSetsCount = ReadMessageId(stream);

			for (var i = 0; i < eventSetsCount; i++)
			{
				var messageId = ReadMessageId(stream);
				var eventsCount = stream.ReadShort();

				var eventSet = GetEventSet(messageId);

				for (var j = 0; j < eventsCount; j++)
				{
					var order = stream.ReadShort();
					var channel = stream.ReadShort();
					eventSet.ReadApproved(tick, order, channel, stream);
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
					inputsSet.ReadFullInput(tick, channel, stream);
				}
			}
		}

		public void WriteOneInput(IEventSet eventSet, int tick, int order, Stream stream)
		{
			var messageId = _inputIdentifiers.GetEventId(eventSet.EventType);

			WriteMessageId(messageId, stream);
			stream.WriteInt(tick);
			eventSet.Write(tick, order, stream);

			// Don't writing channel because server already knows it.
			// Don't writing order because server will append this event and use its own ordering.
		}

		public void WriteOneInput(IInputSet inputSet, int tick, int channel, Stream stream)
		{
			var messageId = _inputIdentifiers.GetInputId(inputSet.InputType);

			WriteMessageId(messageId, stream);
			stream.WriteInt(tick);
			inputSet.Write(tick, channel, stream);

			// Don't writing channel because server already knows it.
		}
	}
}
