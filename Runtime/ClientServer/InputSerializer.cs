using System;
using System.IO;
using Massive.Serialization;

namespace Massive.Netcode
{
	public class InputSerializer
	{
		private readonly InputIdentifiers _inputIdentifiers;
		private readonly Inputs _inputs;

		private IInputSet[] _inputLookupById = Array.Empty<IInputSet>();
		private IEventSet[] _eventLookupById = Array.Empty<IEventSet>();

		public InputSerializer(Inputs inputs, InputIdentifiers inputIdentifiers)
		{
			_inputIdentifiers = inputIdentifiers;
			_inputs = inputs;
		}

		public void Read(int messageId, Stream stream)
		{
			if (_inputIdentifiers.IsEvent(messageId))
			{
				GetEventSet(messageId).Read(stream);
			}
			else
			{
				GetInputSet(messageId).Read(stream);
			}
		}

		public void ReadMany(Stream stream)
		{
			var messagesCount = stream.ReadInt();

			for (var i = 0; i < messagesCount; i++)
			{
				var messageId = SerializationUtils.ReadByte(stream);

				Read(messageId, stream);
			}
		}

		public void WriteFullSync(Stream stream)
		{
			throw new NotImplementedException();
		}

		public IInputSet GetInputSet(int messageId)
		{
			EnsureLookupByIdAt(messageId);

			var candidate = _inputLookupById[messageId];

			if (candidate == null)
			{
				candidate = _inputs.GetInputSetReflected(_inputIdentifiers.GetTypeById(messageId));
				_inputLookupById[messageId] = candidate;
			}

			return candidate;
		}
		
		public IEventSet GetEventSet(int messageId)
		{
			EnsureLookupByIdAt(messageId);

			var candidate = _eventLookupById[messageId];

			if (candidate == null)
			{
				candidate = _inputs.GetEventSetReflected(_inputIdentifiers.GetTypeById(messageId));
				_eventLookupById[messageId] = candidate;
			}

			return candidate;
		}

		private void EnsureLookupByIdAt(int index)
		{
			if (index >= _inputLookupById.Length)
			{
				_inputLookupById = _inputLookupById.ResizeToNextPowOf2(index + 1);
				_eventLookupById = _eventLookupById.Resize(_inputLookupById.Length);
			}
		}
	}
}
