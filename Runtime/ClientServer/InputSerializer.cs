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
					var messageId = SerializationUtils.ReadByte(stream);
					ReadOne(messageId, tick, stream);
				}
			}
			else
			{
				for (var i = 0; i < messagesCount; i++)
				{
					var messageId = SerializationUtils.ReadByte(stream);
					SkipOne(messageId, stream);
				}
			}
		}

		public void WriteMany(int tick, Stream stream)
		{
			throw new NotImplementedException();
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
