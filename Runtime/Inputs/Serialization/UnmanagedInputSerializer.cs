using System;
using System.IO;
using System.Runtime.InteropServices;
using Massive.Serialization;

namespace Massive.Netcode
{
	public unsafe class UnmanagedInputSerializer<T> : IInputSerializer<T> where T : IInput
	{
		private readonly int _dataSize;
		private readonly T[] _dataBuffer;
		private GCHandle _dataBufferHandle;
		private readonly void* _dataBufferPtr;

		public UnmanagedInputSerializer()
		{
			_dataSize = ReflectionUtils.SizeOfUnmanaged(typeof(T));
			_dataBuffer = new T[1];
			_dataBufferHandle = GCHandle.Alloc(_dataBuffer, GCHandleType.Pinned);
			_dataBufferPtr = _dataBufferHandle.AddrOfPinnedObject().ToPointer();
		}

		~UnmanagedInputSerializer()
		{
			_dataBufferHandle.Free();
		}

		public int DataSize => _dataSize;

		public int FullInputSize => _dataSize + sizeof(int);

		public void Write(T data, Stream stream)
		{
			_dataBuffer[0] = data;
			stream.Write(new Span<byte>(_dataBufferPtr, _dataSize));
		}

		public void WriteFullInput(Input<T> data, Stream stream)
		{
			_dataBuffer[0] = data.LastFreshInput;
			stream.Write(new Span<byte>(_dataBufferPtr, _dataSize));
			stream.WriteInt(data.TicksPassed);
		}

		public T Read(Stream stream)
		{
			stream.ReadExactly(new Span<byte>(_dataBufferPtr, _dataSize));
			return _dataBuffer[0];
		}

		public Input<T> ReadFullInput(Stream stream)
		{
			stream.ReadExactly(new Span<byte>(_dataBufferPtr, _dataSize));
			var lastFreshInput = _dataBuffer[0];
			var ticksPassed = stream.ReadInt();
			return new Input<T>(lastFreshInput, ticksPassed, true);
		}
	}
}
