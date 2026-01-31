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
		}

		public IEventSerializer<T> Serializer { get; private set; }

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
		public void SetActual(int tick, int localOrder, T data)
		{
			PopulateUpTo(tick);

			_events[tick].SetActual(localOrder, data);

			_globalChangeTracker.NotifyChange(tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendActual(int tick, T data)
		{
			var localOrder = _events[tick].AppendActual(data);

			_globalChangeTracker.NotifyChange(tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AppendPrediction(int tick, T data)
		{
			var localOrder = _events[tick].AppendPrediction(data);

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

		public void Read(Stream stream)
		{
			throw new System.NotImplementedException();
		}

		public void Write(int tick, int localOrder, Stream stream)
		{
			throw new System.NotImplementedException();
		}

		public void WriteAll(int tick, Stream stream)
		{
			throw new System.NotImplementedException();
		}
	}
}
