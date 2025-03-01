using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace Massive.Netcode
{
	[Il2CppSetOption(Option.NullChecks, false)]
	[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
	[Il2CppSetOption(Option.DivideByZeroChecks, false)]
	public sealed class EventSet<T> : IInputSet
	{
		private readonly ChangeTracker _globalChangeTracker;
		private readonly CyclicList<AllEvents<T>> _events;

		public EventSet(ChangeTracker globalChangeTracker, int startTick)
		{
			_globalChangeTracker = globalChangeTracker;
			_events = new CyclicList<AllEvents<T>>(startTick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AllEvents<T> GetEvents(int tick)
		{
			if (_events.CycledCount == 0)
			{
				return AllEvents<T>.Empty;
			}

			return _events[tick];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ApplyEvent(int tick, int channel, T data)
		{
			_events[tick].Apply(new Event<T>(channel, data));

			_globalChangeTracker.NotifyChange(tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PopulateUpTo(int tick)
		{
			for (var i = _events.TailIndex; i <= tick; ++i)
			{
				ref var events = ref _events.Append();
				events.EnsureInit();
				events.Clear();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DiscardUpTo(int tick)
		{
			_events.RemoveUpTo(tick);
		}

		public void Reevaluate()
		{
			// Events are not predicted, nothing to reevaluate.
		}
	}
}
