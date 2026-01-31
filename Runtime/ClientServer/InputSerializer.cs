using System;
using System.IO;
using Massive.Serialization;

namespace Massive.Netcode
{
	public partial class InputSerializer
	{
		private readonly InputIdentifiers _inputIdentifiers;
		private readonly Inputs _inputs;
		private readonly Func<int, bool> _acceptTick;

		public InputSerializer(Inputs inputs, InputIdentifiers inputIdentifiers, Func<int, bool> acceptTick = null)
		{
			_inputIdentifiers = inputIdentifiers;
			_inputs = inputs;
			_acceptTick = acceptTick ?? (_ => true);
		}

		public void Read(int messageId, Stream stream)
		{
			var tick = stream.ReadInt();

			if (_acceptTick(tick))
			{
				ReadOne(messageId, tick, stream);
			}
			else
			{
				SkipOne(messageId, stream);
			}
		}

		public void ReadMany(Stream stream)
		{
			var messagesCount = stream.ReadInt();
			var tick = stream.ReadInt();

			if (_acceptTick(tick))
			{
				for (var i = 0; i < messagesCount; i++)
				{
					var messageId = stream.Read1Byte();
					ReadOne(messageId, tick, stream);
				}
			}
			else
			{
				for (var i = 0; i < messagesCount; i++)
				{
					var messageId = stream.Read1Byte();
					SkipOne(messageId, stream);
				}
			}
		}

		public void WriteMany(int tick, Stream stream)
		{
			var messagesCount = _inputs.EventSets.Count + _inputs.InputSets.Count;

			stream.WriteInt(messagesCount);
			stream.WriteInt(tick);

			foreach (var eventSet in _inputs.EventSets)
			{
				var messageId = _inputIdentifiers.GetEventId(eventSet.EventType);
				stream.Write1Byte((byte)messageId);
				eventSet.WriteAll(tick, stream);
			}

			foreach (var inputSet in _inputs.InputSets)
			{
				var messageId = _inputIdentifiers.GetInputId(inputSet.InputType);
				stream.Write1Byte((byte)messageId);
				inputSet.WriteAll(tick, stream);
			}
		}

		private void ReadOne(int messageId, int tick, Stream stream)
		{
			if (_inputIdentifiers.IsEvent(messageId))
			{
				GetEventSet(messageId).Read(tick, stream);
			}
			else
			{
				GetInputSet(messageId).Read(tick, stream);
			}
		}

		private void SkipOne(int messageId, Stream stream)
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
	}
}
