using System;
using System.IO;
using System.Runtime.InteropServices;
using Massive.Serialization;

namespace Massive.Netcode
{
	public unsafe class UnmanagedEventSerializer<T> : IEventSerializer where T : IEvent
	{
		private readonly EventSet<T> _eventSet;

		private readonly int _actualEventSize;
		private T[] _actualBuffer;
		private GCHandle _actualBufferHandle;
		private void* _actualBufferPtr;

		public UnmanagedEventSerializer(EventSet<T> eventSet)
		{
			_eventSet = eventSet;

			_actualEventSize = ReflectionUtils.SizeOfUnmanaged(typeof(T));
			_actualBuffer = new T[1];
			_actualBufferHandle = GCHandle.Alloc(_actualBuffer, GCHandleType.Pinned);
			_actualBufferPtr = _actualBufferHandle.AddrOfPinnedObject().ToPointer();
		}

		~UnmanagedEventSerializer()
		{
			_actualBufferHandle.Free();
		}

		public void WriteOne(int tick, int localOrder, Stream stream)
		{
			SerializationUtils.WriteInt(tick, stream);
			SerializationUtils.WriteShort((short)localOrder, stream);

			_actualBuffer[0] = _eventSet.GetAllEvents(tick).Events[localOrder];

			stream.Write(new Span<byte>(_actualBufferPtr, _actualEventSize));
		}

		public void WriteFullSync(int tick, Stream stream)
		{
			var eventsCount = (short)_eventSet.GetAllEvents(tick).Count;
			SerializationUtils.WriteShort((short)eventsCount, stream);

			for (var i = 0; i < eventsCount; i++)
			{
				WriteOne(tick, i, stream);
			}
		}

		public unsafe void ReadOne(Stream stream)
		{
			var tick = SerializationUtils.ReadInt(stream);
			var localOrder = SerializationUtils.ReadShort(stream);

			SerializationUtils.ReadExactly(stream, new Span<byte>(_actualBufferPtr, _actualEventSize));

			_eventSet.SetActual(tick, localOrder, _actualBuffer[0]);
		}

		public unsafe void ReadFullSync(Stream stream)
		{
			var eventsCount = SerializationUtils.ReadShort(stream);

			for (var i = 0; i < eventsCount; i++)
			{
				ReadOne(stream);
			}
		}
	}
}
