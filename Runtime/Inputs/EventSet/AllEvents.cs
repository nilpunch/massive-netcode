using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace Massive.Netcode
{
	/// <summary>
	/// Abscence of ChannelId makes it feel bad.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Il2CppSetOption(Option.NullChecks, false)]
	[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
	public struct AllEvents<T> where T : IEvent
	{
		public int SparseCount { get; private set; }

		public T[] Events { get; private set; }
		public int EventsCapacity { get; private set; }

		public ulong[] AllMask { get; private set; }
		public ulong[] PredictionMask { get; private set; }

		public int MaskLength => (SparseCount + 63) >> 6;

		public bool HasAny => SparseCount != 0;

		public static AllEvents<T> Empty => new AllEvents<T>
		{
			Events = Array.Empty<T>(),
			AllMask = Array.Empty<ulong>(),
			PredictionMask = Array.Empty<ulong>()
		};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EnsureInitialized()
		{
			Events ??= Array.Empty<T>();
			AllMask ??= Array.Empty<ulong>();
			PredictionMask ??= Array.Empty<ulong>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int AppendPrediction(T data)
		{
			var localOrder = SparseCount++;

			EnsureEventAt(localOrder);

			var maskIndex = localOrder >> 6;
			var maskBit = 1UL << (localOrder & 63);

			AllMask[maskIndex] |= maskBit;
			PredictionMask[maskIndex] |= maskBit;

			Events[localOrder] = data;

			return localOrder;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int AppendActual(T data)
		{
			var localOrder = SparseCount++;
			SetActual(localOrder, data);
			return localOrder;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetActual(int localOrder, T data)
		{
			EnsureEventAt(localOrder);

			var maskIndex = localOrder >> 6;
			var maskBit = 1UL << (localOrder & 63);

			var hasEvent = (AllMask[maskIndex] & maskBit) != 0;
			var hasPrediction = (PredictionMask[maskIndex] & maskBit) != 0;

			if (hasEvent && !hasPrediction)
			{
				throw new InvalidOperationException($"You are trying to override actual event with local order {localOrder}.");
			}

			AllMask[maskIndex] |= maskBit;

			// If there is prediction, we need to move it somewhere else, while keeping the order.
			if (hasPrediction)
			{
				var latestPredictionMaskIndex = MaskLength;
				while (--latestPredictionMaskIndex >= 0 && PredictionMask[latestPredictionMaskIndex] == 0)
				{
				}
				var latestPredictionIndex = (latestPredictionMaskIndex << 6) + MathUtils.MSB(AllMask[latestPredictionMaskIndex]);

				// Move head prediction further.
				AppendPrediction(Events[latestPredictionIndex]);

				for (var i = latestPredictionIndex - 1; i >= 0; i--)
				{
					if ((PredictionMask[i >> 6] & (1UL << (i & 63))) != 0)
					{
						Events[latestPredictionIndex] = Events[i];
						latestPredictionIndex = i;
					}
				}

				PredictionMask[maskIndex] &= ~maskBit;
			}
			else
			{
				SparseCount = MathUtils.Max(SparseCount, localOrder + 1);
			}

			Events[localOrder] = data;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			var usedMaskLength = MaskLength;

			for (var i = 0; i < usedMaskLength; i++)
			{
				AllMask[i] = 0;
				PredictionMask[i] = 0;
			}

			SparseCount = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ClearPrediction()
		{
			var usedMaskLength = MaskLength;

			for (var i = 0; i < usedMaskLength; i++)
			{
				AllMask[i] &= ~PredictionMask[i];
				PredictionMask[i] = 0;
			}
		}

		public int DenseCount()
		{
			var count = 0;

			for (var i = 0; i < MaskLength; i++)
			{
				count += MathUtils.PopCount(AllMask[i]);
			}

			return count;
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

			var maskLength = (EventsCapacity + 63) >> 6;
			if (maskLength > AllMask.Length)
			{
				AllMask = AllMask.Resize(maskLength);
				PredictionMask = PredictionMask.Resize(maskLength);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(AllEvents<T> other)
		{
			EnsureEventAt(other.SparseCount - 1);
			Array.Copy(other.Events, Events, other.SparseCount);
			for (var i = 0; i < other.MaskLength; i++)
			{
				AllMask[i] = other.AllMask[i];
				PredictionMask[i] = other.PredictionMask[i];
			}
			SparseCount = other.SparseCount;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AllEventsEnumerator<T> GetEnumerator()
		{
			return new AllEventsEnumerator<T>(this);
		}
	}
}
