using System.IO;
using Massive.Serialization;

namespace Massive.Netcode
{
	public partial class MessageSerializer
	{
		protected readonly InputIdentifiers _inputIdentifiers;

		protected readonly Inputs _inputs;

		public MessageSerializer(Inputs inputs, InputIdentifiers inputIdentifiers)
		{
			_inputIdentifiers = inputIdentifiers;
			_inputs = inputs;
		}

		public void WriteMessageId(int messageId, Stream stream)
		{
			stream.Write1Byte((byte)messageId);
		}

		public void WriteMessageId(MessageType messageType, Stream stream)
		{
			stream.Write1Byte((byte)messageType);
		}

		public void UndoMessageIdRead(Stream stream)
		{
			stream.Position -= 1;
		}

		public int ReadMessageId(Stream stream)
		{
			return stream.Read1Byte();
		}
	}
}
