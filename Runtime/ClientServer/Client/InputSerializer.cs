using System;
using System.IO;
using Massive.Serialization;

namespace Massive.Netcode
{
	public class InputSerializer
	{
		private readonly InputIdentifiers _inputIdentifiers;
		private readonly Inputs _inputs;

		private IInputSet[] _lookupById = Array.Empty<IInputSet>();

		public InputSerializer(Inputs inputs, InputIdentifiers inputIdentifiers)
		{
			_inputIdentifiers = inputIdentifiers;
			_inputs = inputs;
		}

		public void ReadActualInput(int messageId, Stream stream)
		{
			if (!_inputIdentifiers.IsRegistered(messageId))
			{
				return;
			}

			GetInputSet(messageId).ReadActualInput(stream);
		}

		public void ReadActualAndPredictionInputs(Stream stream)
		{
			var messagesCount = SerializationUtils.ReadInt(stream);

			for (var i = 0; i < messagesCount; i++)
			{
				var messageId = SerializationUtils.ReadByte(stream);

				if (!_inputIdentifiers.IsRegistered(messageId))
				{
					continue;
				}

				GetInputSet(messageId).ReadActualAndPredictionInput(stream);
			}
		}

		private IInputSet GetInputSet(int messageId)
		{
			EnsureLookupByIdAt(messageId);

			var candidate = _lookupById[messageId];

			if (candidate == null)
			{
				candidate = _inputIdentifiers.IsEvent(messageId)
					? _inputs.GetEventSetReflected(_inputIdentifiers.GetTypeById(messageId))
					: _inputs.GetInputSetReflected(_inputIdentifiers.GetTypeById(messageId));
				_lookupById[messageId] = candidate;
			}
			return candidate;
		}

		private void EnsureLookupByIdAt(int index)
		{
			if (index >= _lookupById.Length)
			{
				_lookupById = _lookupById.ResizeToNextPowOf2(index + 1);
			}
		}
	}
}
