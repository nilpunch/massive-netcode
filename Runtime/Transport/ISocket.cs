using System;
using System.Buffers;

namespace Massive.Netcode
{
	public interface ISocket : IDisposable
	{
		bool IsConnected { get; }

		bool HasUnreadPayload { get; }

		/// <summary>
		/// Operation is not immediate. Check <see cref="IsConnected"/> for connection status.
		/// </summary>
		void Connect(Uri uri);

		/// <summary>
		/// Operation is not immediate. Check <see cref="IsConnected"/> for connection status.
		/// </summary>
		void Disconnect();

		void ReadPayload(ref SequenceReader<byte> reader);

		void Send(IBufferWriter<byte> writer);
	}
}
