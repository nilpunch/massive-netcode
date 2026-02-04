using System;
using System.IO;
using System.Runtime.InteropServices;
using Massive.Serialization;

namespace Massive.Netcode
{
	public unsafe class UnmanagedEventSerializer<T> : IEventSerializer<T> where T : IEvent
	{
		private readonly int _dataSize;
		private T[] _dataBuffer;
		private GCHandle _dataBufferHandle;
		private void* _dataBufferPtr;

		public UnmanagedEventSerializer()
		{
			_dataSize = ReflectionUtils.SizeOfUnmanaged(typeof(T));
			_dataBuffer = new T[1];
			_dataBufferHandle = GCHandle.Alloc(_dataBuffer, GCHandleType.Pinned);
			_dataBufferPtr = _dataBufferHandle.AddrOfPinnedObject().ToPointer();
		}

		~UnmanagedEventSerializer()
		{
			_dataBufferHandle.Free();
		}

		public int DataSize => _dataSize;

		public void Write(T data, Stream stream)
		{
			_dataBuffer[0] = data;
			stream.Write(new Span<byte>(_dataBufferPtr, _dataSize));
		}

		public T Read(Stream stream)
		{
			stream.ReadExactly(new Span<byte>(_dataBufferPtr, _dataSize));
			return _dataBuffer[0];
		}
	}
}
