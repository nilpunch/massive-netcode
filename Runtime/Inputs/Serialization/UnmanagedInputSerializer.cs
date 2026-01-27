using System;
using System.IO;
using System.Runtime.InteropServices;
using Massive.Serialization;

namespace Massive.Netcode
{
	public unsafe class UnmanagedInputSerializer<T> : IInputSerializer where T : IInput
	{
		private readonly InputSet<T> _inputSet;

		private readonly int _actualInputSize;
		private ActualInput<T>[] _actualBuffer;
		private GCHandle _actualBufferHandle;
		private void* _actualBufferPtr;

		private readonly int _fullSyncInputSize;
		private Input<T>[] _fullSyncBuffer;
		private GCHandle _fullSyncBufferHandle;
		private void* _fullSyncBufferPtr;

		private AllInputs<T> _allInputsBuffer;

		public UnmanagedInputSerializer(InputSet<T> inputSet)
		{
			_inputSet = inputSet;

			_actualInputSize = ReflectionUtils.SizeOfUnmanaged(typeof(ActualInput<T>));
			_actualBuffer = Array.Empty<ActualInput<T>>();
			_actualBufferHandle = GCHandle.Alloc(_fullSyncBuffer, GCHandleType.Pinned);
			_actualBufferPtr = _actualBufferHandle.AddrOfPinnedObject().ToPointer();

			_fullSyncInputSize = ReflectionUtils.SizeOfUnmanaged(typeof(Input<T>));
			_fullSyncBuffer = Array.Empty<Input<T>>();
			_fullSyncBufferHandle = GCHandle.Alloc(_fullSyncBuffer, GCHandleType.Pinned);
			_fullSyncBufferPtr = _fullSyncBufferHandle.AddrOfPinnedObject().ToPointer();

		}

		~UnmanagedInputSerializer()
		{
			_actualBufferHandle.Free();
			_fullSyncBufferHandle.Free();
		}

		public void ReadActual(Stream stream)
		{
			var tick = SerializationUtils.ReadInt(stream);
			var inputsAmount = SerializationUtils.ReadShort(stream);

			EnsureActualBufferSize(inputsAmount);
			var actualSpan = new Span<byte>(_actualBufferPtr, _actualInputSize * inputsAmount);
			SerializationUtils.ReadExactly(stream, actualSpan);

			for (var i = 0; i < inputsAmount; i++)
			{
				_inputSet.SetActual(tick, _actualBuffer[i].Channel, _actualBuffer[i].Input);
			}
		}

		public void ReadFullSync(Stream stream)
		{
			var tick = SerializationUtils.ReadInt(stream);
			var channelsAmount = SerializationUtils.ReadShort(stream);

			EnsureFullSyncBufferSize(channelsAmount);
			var fullSyncSpan = new Span<byte>(_fullSyncBufferPtr, _fullSyncInputSize * channelsAmount);
			SerializationUtils.ReadExactly(stream, fullSyncSpan);

			_allInputsBuffer.EnsureInitialized();
			_allInputsBuffer.EnsureChannel(channelsAmount - 1);
			for (var i = 0; i < channelsAmount; i++)
			{
				_allInputsBuffer.Inputs[i] = _fullSyncBuffer[i];
			}

			_inputSet.SetInputs(tick, _allInputsBuffer);
			_allInputsBuffer.Clear();
		}

		private void EnsureActualBufferSize(int length)
		{
			if (length > _actualBuffer.Length)
			{
				_actualBuffer = _actualBuffer.ResizeToNextPowOf2(length);
				_actualBufferHandle.Free();
				_actualBufferHandle = GCHandle.Alloc(_actualBuffer, GCHandleType.Pinned);
				_actualBufferPtr = _actualBufferHandle.AddrOfPinnedObject().ToPointer();
			}
		}

		private void EnsureFullSyncBufferSize(int length)
		{
			if (length > _fullSyncBuffer.Length)
			{
				_fullSyncBuffer = _fullSyncBuffer.ResizeToNextPowOf2(length);
				_fullSyncBufferHandle.Free();
				_fullSyncBufferHandle = GCHandle.Alloc(_fullSyncBuffer, GCHandleType.Pinned);
				_fullSyncBufferPtr = _fullSyncBufferHandle.AddrOfPinnedObject().ToPointer();
			}
		}
	}
}
