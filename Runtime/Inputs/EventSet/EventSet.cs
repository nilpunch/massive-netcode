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
		public void SetApproved(int tick, int order, int channel, T data)
		{
			PopulateUpTo(tick);

			_events[tick].SetApproved(order, channel, data);

			_globalChangeTracker.NotifyChange(tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendApproved(int tick, int channel, T data)
		{
			_events[tick].AppendApproved(channel, data);

			_globalChangeTracker.NotifyChange(tick);
		}

		int IEventSet.AppendApprovedDefault(int tick, int channel)
		{
			var order = _events[tick].AppendApproved(channel, default);

			_globalChangeTracker.NotifyChange(tick);

			return order;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendPrediction(int tick, int channel, T data)
		{
			var order = _events[tick].AppendPrediction(channel, data);

			_globalChangeTracker.NotifyChange(tick);
			_predictionReceiver?.OnEventPredicted(this, tick, order);
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

		public void ReadApproved(int tick, int order, int channel, Stream stream)
		{
			PopulateUpTo(tick);

			_events[tick].SetApproved(order, channel, Serializer.Read(stream));
		}

		public void Write(int tick, int order, Stream stream)
		{
			Serializer.Write(_events[tick].Events[order].Data, stream);
		}

		public void Skip(Stream stream)
		{
			Serializer.Read(stream);
		}

		public int GetEventsCount(int tick)
		{
			return _events[tick].DenseCount();
		}

		public int GetEventChannel(int tick, int order)
		{
			return _events[tick].Events[order].Channel;
		}

		MaskEnumerator IEventSet.GetAllEvents(int tick)
		{
			ref var events = ref _events[tick];
			return new MaskEnumerator(events.AllMask, events.MaskLength);
		}
	}
}
