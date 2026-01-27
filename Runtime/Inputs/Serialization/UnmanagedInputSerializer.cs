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
		private T[] _actualBuffer;
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

			_actualInputSize = ReflectionUtils.SizeOfUnmanaged(typeof(T));
			_actualBuffer = new T[1];
			_actualBufferHandle = GCHandle.Alloc(_actualBuffer, GCHandleType.Pinned);
			_actualBufferPtr = _actualBufferHandle.AddrOfPinnedObject().ToPointer();

			_fullSyncInputSize = ReflectionUtils.SizeOfUnmanaged(typeof(Input<T>));
			_fullSyncBuffer = new Input<T>[1];
			_fullSyncBufferHandle = GCHandle.Alloc(_fullSyncBuffer, GCHandleType.Pinned);
			_fullSyncBufferPtr = _fullSyncBufferHandle.AddrOfPinnedObject().ToPointer();
		}

		~UnmanagedInputSerializer()
		{
			_actualBufferHandle.Free();
			_fullSyncBufferHandle.Free();
		}

		public void WriteOne(int tick, int channel, Stream stream)
		{
			SerializationUtils.WriteInt(tick, stream);
			SerializationUtils.WriteShort((short)channel, stream);

			_actualBuffer[0] = _inputSet.GetInputs(tick).Inputs[channel].LastFreshInput;

			stream.Write(new Span<byte>(_actualBufferPtr, _actualInputSize));
		}

		public void WriteFullSync(int tick, Stream stream)
		{
			var inputs = _inputSet.GetInputs(tick);

			SerializationUtils.WriteInt(tick, stream);
			SerializationUtils.WriteShort((short)inputs.UsedChannels, stream);

			EnsureFullSyncBufferSize(inputs.UsedChannels);
			for (var i = 0; i < inputs.UsedChannels; i++)
			{
				_fullSyncBuffer[i] = _allInputsBuffer.Inputs[i];
			}

			stream.Write(new Span<byte>(_fullSyncBufferPtr, _fullSyncInputSize * inputs.UsedChannels));
		}

		public void ReadOne(Stream stream)
		{
			var tick = SerializationUtils.ReadInt(stream);
			var channel = SerializationUtils.ReadShort(stream);

			SerializationUtils.ReadExactly(stream, new Span<byte>(_actualBufferPtr, _actualInputSize));

			_inputSet.SetActual(tick, channel, _actualBuffer[0]);
		}

		public void ReadFullSync(Stream stream)
		{
			var tick = SerializationUtils.ReadInt(stream);
			var channelsCount = SerializationUtils.ReadShort(stream);

			EnsureFullSyncBufferSize(channelsCount);
			var fullSyncSpan = new Span<byte>(_fullSyncBufferPtr, _fullSyncInputSize * channelsCount);
			SerializationUtils.ReadExactly(stream, fullSyncSpan);

			_allInputsBuffer.EnsureInitialized();
			_allInputsBuffer.EnsureChannel(channelsCount - 1);
			for (var i = 0; i < channelsCount; i++)
			{
				_allInputsBuffer.Inputs[i] = _fullSyncBuffer[i];
			}

			_inputSet.SetInputs(tick, _allInputsBuffer);
			_allInputsBuffer.Clear();
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
