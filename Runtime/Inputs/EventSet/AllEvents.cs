using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace Massive.Netcode
{
	[Il2CppSetOption(Option.NullChecks, false)]
	[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
	public struct AllEvents<T> where T : IEvent
	{
		public int Count { get; private set; }

		public T[] Events { get; private set; }
		public ulong[] AppliedMask { get; private set; }

		public int EventsCapacity { get; private set; }

		public int UsedMaskLength => (Count + 63) >> 6;

		public bool HasAny => Count != 0;

		public static AllEvents<T> Empty => new AllEvents<T>
		{
			Events = Array.Empty<T>(),
		};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EnsureInit()
		{
			Events ??= Array.Empty<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Append(T data)
		{
			var localOrder = Count;
			Apply(localOrder, data);
			return localOrder;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Apply(int localOrder, T data)
		{
			if ((AppliedMask[localOrder >> 6] & (1UL << (localOrder & 63))) != 0)
			{
				throw new InvalidOperationException($"You are trying to override existing event with local order {localOrder}.");
			}
			
			EnsureEventAt(localOrder);

			Events[localOrder] = data;

			AppliedMask[localOrder >> 6] |= 1UL << (localOrder & 63);

			Count = MathUtils.Max(Count, localOrder + 1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			var usedMaskLength = UsedMaskLength;

			for (var i = 0; i < usedMaskLength; i++)
			{
				AppliedMask[i] = 0UL;
			}

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
			AppliedMask = AppliedMask.Resize((EventsCapacity + 63) >> 6);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(AllEvents<T> other)
		{
			EnsureEventAt(other.Count - 1);
			Array.Copy(other.Events, Events, other.Count);
			Array.Copy(other.AppliedMask, AppliedMask, other.UsedMaskLength);
			Count = other.Count;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AllEventsEnumerator<T> GetEnumerator()
		{
			return new AllEventsEnumerator<T>(this);
		}
	}
}
