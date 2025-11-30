using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace Massive.Netcode
{
	[Il2CppSetOption(Option.NullChecks, false)]
	[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
	public struct AllEvents<T>
	{
		public int Count { get; private set; }

		public Event<T>[] Events { get; private set; }

		public int EventsCapacity { get; private set; }

		public bool HasAny => Count != 0;

		public static AllEvents<T> Empty => new AllEvents<T>
		{
			Events = Array.Empty<Event<T>>(),
		};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EnsureInit()
		{
			Events ??= Array.Empty<Event<T>>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Apply(Event<T> @event)
		{
			if (@event.Channel < 0)
			{
				throw new InvalidOperationException("Can't have negative channel.");
			}

			EnsureEventAt(Count);

			var insertionIndex = Array.BinarySearch(Events, 0, Count, @event, Event<T>.ChannelComparer.Instance);
			if (insertionIndex >= 0) // Update the event.
			{
				Events[insertionIndex] = @event;
				return;
			}

			insertionIndex = ~insertionIndex;
			if (insertionIndex < Count)
			{
				Array.Copy(Events, insertionIndex, Events, insertionIndex + 1, Count - insertionIndex);
			}

			Events[insertionIndex] = @event;
			Count += 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			Count = 0;
		}

		/// <summary>
		/// Ensures the packed array has sufficient capacity for the specified index.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EnsureEventAt(int index)
		{
			if (index >= EventsCapacity)
			{
				ResizeEvents(MathUtils.RoundUpToPowerOfTwo(index + 1));
			}
		}

		/// <summary>
		/// Resizes the packed array to the specified capacity.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ResizeEvents(int capacity)
		{
			Events = Events.Resize(capacity);
			EventsCapacity = capacity;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AllEventsEnumerator<T> GetEnumerator()
		{
			return new AllEventsEnumerator<T>(this);
		}
	}
}
