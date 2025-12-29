using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace Massive.Netcode
{
	[Il2CppSetOption(Option.NullChecks, false)]
	[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
	public sealed class EventSet<T> : IInputSet
	{
		private readonly ChangeTracker _globalChangeTracker;
		private readonly IInputReceiver _inputReceiver;
		private readonly CyclicList<AllEvents<T>> _events;

		public EventSet(ChangeTracker globalChangeTracker, int startTick, IInputReceiver inputReceiver = null)
		{
			_globalChangeTracker = globalChangeTracker;
			_inputReceiver = inputReceiver;
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
			_inputReceiver?.ApplyEventAt(tick, channel, data);
		}

		public void PopulateUpTo(int tick)
		{
			for (var i = _events.TailIndex; i <= tick; ++i)
			{
				ref var events = ref _events.Append();
				events.EnsureInit();
				events.Clear();
			}
		}

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
