using System;
using System.IO;
using System.Runtime.InteropServices;
using Massive.Serialization;

namespace Massive.Netcode
{
	public sealed partial class InputSet<T>
	{
		private readonly int _actualInputSize = ReflectionUtils.SizeOfUnmanaged(typeof(T));
		private readonly int _fullSyncInputSize = ReflectionUtils.SizeOfUnmanaged(typeof(Input<T>));
		private Input<T>[] _fullSyncBuffer;
		private GCHandle _fullSyncBufferHandle;
		private AllInputs<T> _allInputsBuffer;

		~InputSet()
		{
			_fullSyncBufferHandle.Free();
		}

		public unsafe void ReadActualInput(Stream stream)
		{
			throw new NotImplementedException();

			var tick = SerializationUtils.ReadInt(stream);
			var channelsAmount = SerializationUtils.ReadInt(stream);

			EnsureDeserializationBufferSize(channelsAmount);
			var inputsAsSpan = new Span<byte>(_fullSyncBufferHandle.AddrOfPinnedObject().ToPointer(), _fullSyncInputSize * channelsAmount);
			SerializationUtils.ReadExactly(stream, inputsAsSpan);

			_allInputsBuffer.EnsureInitialized();
			_allInputsBuffer.EnsureChannel(channelsAmount - 1);
			for (var i = 0; i < channelsAmount; i++)
			{
				_allInputsBuffer.Inputs[i] = _fullSyncBuffer[i];
			}

			SetInputs(tick, _allInputsBuffer);
			_allInputsBuffer.Clear();
		}

		public unsafe void ReadFullSyncInput(Stream stream)
		{
			var tick = SerializationUtils.ReadInt(stream);
			var channelsAmount = SerializationUtils.ReadInt(stream);

			EnsureDeserializationBufferSize(channelsAmount);
			var inputsAsSpan = new Span<byte>(_fullSyncBufferHandle.AddrOfPinnedObject().ToPointer(), _fullSyncInputSize * channelsAmount);
			SerializationUtils.ReadExactly(stream, inputsAsSpan);

			_allInputsBuffer.EnsureInitialized();
			_allInputsBuffer.EnsureChannel(channelsAmount - 1);
			for (var i = 0; i < channelsAmount; i++)
			{
				_allInputsBuffer.Inputs[i] = _fullSyncBuffer[i];
			}

			SetInputs(tick, _allInputsBuffer);
			_allInputsBuffer.Clear();
		}

		private void EnsureDeserializationBufferSize(int length)
		{
			if (_fullSyncBuffer == null)
			{
				_fullSyncBuffer = new Input<T>[length];
				_fullSyncBufferHandle = GCHandle.Alloc(_fullSyncBuffer, GCHandleType.Pinned);
			}
			else if (length > _fullSyncBuffer.Length)
			{
				_fullSyncBuffer = _fullSyncBuffer.ResizeToNextPowOf2(length);
				_fullSyncBufferHandle.Free();
				_fullSyncBufferHandle = GCHandle.Alloc(_fullSyncBuffer, GCHandleType.Pinned);
			}
		}
	}
}
