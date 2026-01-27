using System;
using System.IO;
using Massive.Serialization;

namespace Massive.Netcode
{
	public class InputSerializer
	{
		private readonly InputIdentifiers _inputIdentifiers;
		private readonly Inputs _inputs;

		private IInputSerializer[] _lookupById = Array.Empty<IInputSerializer>();

		public InputSerializer(Inputs inputs, InputIdentifiers inputIdentifiers)
		{
			_inputIdentifiers = inputIdentifiers;
			_inputs = inputs;
		}

		public void ReadActualInput(int messageId, Stream stream)
		{
			GetInputSerializer(messageId).ReadActual(stream);
		}

		public void ReadFullSync(Stream stream)
		{
			var messagesCount = SerializationUtils.ReadInt(stream);

			for (var i = 0; i < messagesCount; i++)
			{
				var messageId = SerializationUtils.ReadByte(stream);

				GetInputSerializer(messageId).ReadFullSync(stream);
			}
		}

		private IInputSerializer GetInputSerializer(int messageId)
		{
			EnsureLookupByIdAt(messageId);

			var candidate = _lookupById[messageId];

			if (candidate == null)
			{
				candidate = _inputIdentifiers.IsEvent(messageId)
					? _inputs.GetEventSetSerializer(_inputIdentifiers.GetTypeById(messageId))
					: _inputs.GetInputSetSerializer(_inputIdentifiers.GetTypeById(messageId));
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
