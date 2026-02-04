using System;
using System.IO;
using System.Runtime.InteropServices;
using Massive.Serialization;

namespace Massive.Netcode
{
	public unsafe class UnmanagedInputSerializer<T> : IInputSerializer<T> where T : IInput
	{
		private readonly int _dataSize;
		private T[] _dataBuffer;
		private GCHandle _dataBufferHandle;
		private void* _dataBufferPtr;

		private readonly int _inputSize;
		private Input<T>[] _inputBuffer;
		private GCHandle _inputBufferHandle;
		private void* _inputBufferPtr;

		public UnmanagedInputSerializer()
		{
			_dataSize = ReflectionUtils.SizeOfUnmanaged(typeof(T));
			_dataBuffer = new T[1];
			_dataBufferHandle = GCHandle.Alloc(_dataBuffer, GCHandleType.Pinned);
			_dataBufferPtr = _dataBufferHandle.AddrOfPinnedObject().ToPointer();

			_inputSize = ReflectionUtils.SizeOfUnmanaged(typeof(Input<T>));
			_inputBuffer = new Input<T>[1];
			_inputBufferHandle = GCHandle.Alloc(_inputBuffer, GCHandleType.Pinned);
			_inputBufferPtr = _inputBufferHandle.AddrOfPinnedObject().ToPointer();
		}

		~UnmanagedInputSerializer()
		{
			_dataBufferHandle.Free();
			_inputBufferHandle.Free();
		}

		public int DataSize => _dataSize;

		public int InputSize => _inputSize;

		public void WriteData(T data, Stream stream)
		{
			_dataBuffer[0] = data;
			stream.Write(new Span<byte>(_dataBufferPtr, _dataSize));
		}

		public void WriteInput(Input<T> data, Stream stream)
		{
			_inputBuffer[0] = data;
			stream.Write(new Span<byte>(_inputBufferPtr, _inputSize));
		}

		public T ReadData(Stream stream)
		{
			stream.ReadExactly(new Span<byte>(_dataBufferPtr, _dataSize));
			return _dataBuffer[0];
		}

		public Input<T> ReadInput(Stream stream)
		{
			stream.ReadExactly(new Span<byte>(_inputBufferPtr, _inputSize));
			return _inputBuffer[0];
		}
	}
}
