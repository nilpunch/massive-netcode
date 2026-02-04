using System;

namespace Massive.Netcode
{
	public partial class MessageSerializer
	{
		private IInputSet[] _inputLookupById = Array.Empty<IInputSet>();
		private IEventSet[] _eventLookupById = Array.Empty<IEventSet>();

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
