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

		private readonly int _fullInputSize;
		private readonly Input<T>[] _fullInputBuffer;
		private GCHandle _fullInputBufferHandle;
		private readonly void* _fullInputBufferPtr;

		public UnmanagedInputSerializer()
		{
			_dataSize = ReflectionUtils.SizeOfUnmanaged(typeof(T));
			_dataBuffer = new T[1];
			_dataBufferHandle = GCHandle.Alloc(_dataBuffer, GCHandleType.Pinned);
			_dataBufferPtr = _dataBufferHandle.AddrOfPinnedObject().ToPointer();

			_fullInputSize = ReflectionUtils.SizeOfUnmanaged(typeof(Input<T>));
			_fullInputBuffer = new Input<T>[1];
			_fullInputBufferHandle = GCHandle.Alloc(_fullInputBuffer, GCHandleType.Pinned);
			_fullInputBufferPtr = _fullInputBufferHandle.AddrOfPinnedObject().ToPointer();
		}

		~UnmanagedInputSerializer()
		{
			_dataBufferHandle.Free();
			_fullInputBufferHandle.Free();
		}

		public int DataSize => _dataSize;

		public int FullInputSize => _fullInputSize;

		public void Write(T data, Stream stream)
		{
			_dataBuffer[0] = data;
			stream.Write(new Span<byte>(_dataBufferPtr, _dataSize));
		}

		public void WriteFullInput(Input<T> data, Stream stream)
		{
			_fullInputBuffer[0] = data;
			stream.Write(new Span<byte>(_fullInputBufferPtr, _fullInputSize));
		}

		public T Read(Stream stream)
		{
			stream.ReadExactly(new Span<byte>(_dataBufferPtr, _dataSize));
			return _dataBuffer[0];
		}

		public Input<T> ReadFullInput(Stream stream)
		{
			stream.ReadExactly(new Span<byte>(_fullInputBufferPtr, _fullInputSize));
			return _fullInputBuffer[0];
		}
	}
}
