using System;
using System.IO;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace Massive.Netcode
{
	[Il2CppSetOption(Option.NullChecks, false)]
	[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
	public sealed class EventSet<T> : IEventSet where T : IEvent
	{
		private readonly ChangeTracker _globalChangeTracker;
		private readonly IPredictionReceiver _predictionReceiver;
		private readonly CyclicList<AllEvents<T>> _events;

		public EventSet(ChangeTracker globalChangeTracker, int startTick, IPredictionReceiver predictionReceiver = null, IEventSerializer<T> serializer = null)
		{
			_globalChangeTracker = globalChangeTracker;
			_predictionReceiver = predictionReceiver;
			_events = new CyclicList<AllEvents<T>>(startTick);
			Serializer = serializer ?? new UnmanagedEventSerializer<T>();
			EventType = typeof(T);
			EventDataSize = Serializer.DataSize;
		}

		public IEventSerializer<T> Serializer { get; }

		public Type EventType { get; }

		public int EventDataSize { get; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AllEvents<T> GetAllEvents(int tick)
		{
			if (_events.CycledCount == 0)
			{
				return AllEvents<T>.Empty;
			}

			return _events[tick];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetActual(int tick, int localOrder, int channel, T data)
		{
			PopulateUpTo(tick);

			_events[tick].SetActual(localOrder, channel, data);

			_globalChangeTracker.NotifyChange(tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendActual(int tick, int channel, T data)
		{
			_events[tick].AppendActual(channel, data);

			_globalChangeTracker.NotifyChange(tick);
		}

		int IEventSet.AppendActualDefault(int tick, int channel)
		{
			var localOrder = _events[tick].AppendActual(channel, default);

			_globalChangeTracker.NotifyChange(tick);

			return localOrder;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendPrediction(int tick, int channel, T data)
		{
			var localOrder = _events[tick].AppendPrediction(channel, data);

			_globalChangeTracker.NotifyChange(tick);
			_predictionReceiver?.OnEventPredicted(this, tick, localOrder);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetEvents(int tick, AllEvents<T> allEvents)
		{
			PopulateUpTo(tick);

			_events[tick].CopyFrom(allEvents);

			_globalChangeTracker.NotifyChange(tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear(int tick)
		{
			PopulateUpTo(tick);

			_events[tick].Clear();

			_globalChangeTracker.NotifyChange(tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearPrediction(int tick)
		{
			PopulateUpTo(tick);

			_events[tick].ClearPrediction();

			_globalChangeTracker.NotifyChange(tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearPrediction(int startTick, int endTick)
		{
			for (var tick = startTick; tick <= endTick; tick++)
			{
				ClearPrediction(tick);
			}
		}

		public void PopulateUpTo(int tick)
		{
			for (var i = _events.TailIndex; i <= tick; i++)
			{
				ref var events = ref _events.Append();
				events.EnsureInitialized();
				events.Clear();
			}
		}

		public void DiscardUpTo(int tick)
		{
			_events.RemoveUpTo(tick);
		}

		public void Reset(int startTick)
		{
			_events.Reset(startTick);
		}

		public void ReadData(int tick, int localOrder, int channel, Stream stream)
		{
			PopulateUpTo(tick);

			_events[tick].SetActual(localOrder, channel, Serializer.Read(stream));
		}

		public void WriteData(int tick, int localOrder, Stream stream)
		{
			Serializer.Write(_events[tick].Events[localOrder].Data, stream);
		}

		public void SkipData(Stream stream)
		{
			Serializer.Read(stream);
		}

		public int GetEventsCount(int tick)
		{
			return _events[tick].DenseCount();
		}

		public int GetEventChannel(int tick, int localOrder)
		{
			return _events[tick].Events[localOrder].Channel;
		}

		public LocalOrdersEnumerator GetEventsLocalOrders(int tick)
		{
			ref var events = ref _events[tick];
			return new LocalOrdersEnumerator(events.AllMask, events.MaskLength);
		}
	}
}
