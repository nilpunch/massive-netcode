using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace Massive.Netcode
{
	[Il2CppSetOption(Option.NullChecks, false)]
	[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
	[Il2CppSetOption(Option.DivideByZeroChecks, false)]
	public struct AllEventsEnumerator<T>
	{
		private readonly AllEvents<T> _allEvents;
		private int _index;

		public AllEventsEnumerator(AllEvents<T> allEvents)
		{
			_allEvents = allEvents;
			_index = _allEvents.Count;
		}

		public Event<T> Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _allEvents.Events[_index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			return --_index >= 0;
		}
	}
}
