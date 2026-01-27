using System;
using System.IO;
using System.Runtime.InteropServices;
using Massive.Serialization;

namespace Massive.Netcode
{
	public unsafe class UnmanagedEventSerializer<T> : IInputSerializer where T : IEvent
	{
		private readonly EventSet<T> _eventSet;

		private readonly int _actualEventSize;
		private ActualEvent<T>[] _actualBuffer;
		private GCHandle _actualBufferHandle;
		private void* _actualBufferPtr;

		private readonly int _fullSyncEventSize;
		private T[] _fullSyncBuffer;
		private GCHandle _fullSyncBufferHandle;
		private void* _fullSyncBufferPtr;

		private AllEvents<T> _allEventsBuffer;

		public UnmanagedEventSerializer(EventSet<T> eventSet)
		{
			_eventSet = eventSet;

			_actualEventSize = ReflectionUtils.SizeOfUnmanaged(typeof(ActualEvent<T>));
			_actualBuffer = Array.Empty<ActualEvent<T>>();
			_actualBufferHandle = GCHandle.Alloc(_fullSyncBuffer, GCHandleType.Pinned);
			_actualBufferPtr = _actualBufferHandle.AddrOfPinnedObject().ToPointer();

			_fullSyncEventSize = ReflectionUtils.SizeOfUnmanaged(typeof(T));
			_fullSyncBuffer = Array.Empty<T>();
			_fullSyncBufferHandle = GCHandle.Alloc(_fullSyncBuffer, GCHandleType.Pinned);
			_fullSyncBufferPtr = _fullSyncBufferHandle.AddrOfPinnedObject().ToPointer();
		}

		~UnmanagedEventSerializer()
		{
			_actualBufferHandle.Free();
			_fullSyncBufferHandle.Free();
		}

		public unsafe void ReadActual(Stream stream)
		{
			var tick = SerializationUtils.ReadInt(stream);
			var eventsAmount = SerializationUtils.ReadShort(stream);

			EnsureActualBufferSize(eventsAmount);
			var actualSpan = new Span<byte>(_actualBufferPtr, _actualEventSize * eventsAmount);
			SerializationUtils.ReadExactly(stream, actualSpan);

			for (var i = 0; i < eventsAmount; i++)
			{
				_eventSet.SetActual(tick, _actualBuffer[i].LocalOrder, _actualBuffer[i].Event);
			}
		}

		public unsafe void ReadFullSync(Stream stream)
		{
			var tick = SerializationUtils.ReadInt(stream);
			var eventsAmount = SerializationUtils.ReadShort(stream);

			EnsureFullSyncBufferSize(eventsAmount);
			var fullSyncSpan = new Span<byte>(_fullSyncBufferPtr, _fullSyncEventSize * eventsAmount);
			SerializationUtils.ReadExactly(stream, fullSyncSpan);

			_allEventsBuffer.EnsureInitialized();
			for (var i = 0; i < eventsAmount; i++)
			{
				_allEventsBuffer.AppendActual(_fullSyncBuffer[i]);
			}

			_eventSet.SetEvents(tick, _allEventsBuffer);
			_allEventsBuffer.Clear();
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
