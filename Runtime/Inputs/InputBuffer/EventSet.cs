using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace Massive.Netcode
{
	[Il2CppSetOption(Option.NullChecks, false)]
	[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
	[Il2CppSetOption(Option.DivideByZeroChecks, false)]
	public sealed class EventSet<T> : IInput
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
		public void AssignEvent(int tick, Event<T> @event)
		{
			_events[tick].Assign(@event);

			_globalChangeTracker.NotifyChange(tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PopulateInputsUpTo(int tick)
		{
			for (var i = _events.TailIndex; i <= tick; ++i)
			{
				ref var events = ref _events.Append();
				events.EnsureInit();
				events.Clear();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DiscardInputsUpTo(int tick)
		{
			_events.RemoveUpTo(tick);
		}

		public void ReevaluateInputs()
		{
			// Events are not predicted, nothing to reevaluate.
		}
	}
}
