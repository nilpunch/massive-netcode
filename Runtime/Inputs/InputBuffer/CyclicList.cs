﻿using System;
using System.Runtime.CompilerServices;

namespace Massive.Netcode
{
	public class CyclicList<T>
	{
		public T[] Data { get; private set; } = Array.Empty<T>();

		public int CycleCapacity { get; private set; }

		public int CycledCount { get; private set; }

		public int TailIndex { get; private set; }

		public int HeadIndex
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => TailIndex - CycledCount;
		}

		public CyclicList(int startIndex = 0)
		{
			TailIndex = startIndex;
		}

		public ref T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				if (index < HeadIndex || index >= TailIndex)
				{
					throw new ArgumentOutOfRangeException(nameof(index), index, $"List works in range [{HeadIndex}, {TailIndex}).");
				}
				return ref Data[MathUtils.FastMod(index, CycleCapacity)];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(T data)
		{
			EnsureCapacity(CycledCount + 1);

			Data[MathUtils.FastMod(TailIndex, CycleCapacity)] = data;
			TailIndex++;
			CycledCount++;
		}

		/// <summary>
		/// Removes elements from the beginning (head) up to the specified absolute index.
		/// The index must be in the range [HeadIndex, TailIndex].
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveUpTo(int index)
		{
			if (index < HeadIndex)
			{
				index = HeadIndex;
			}
			if (index > TailIndex)
			{
				index = TailIndex;
			}

			var removeCount = index - HeadIndex;
			CycledCount -= removeCount;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reset(int startIndex)
		{
			CycledCount = 0;
			TailIndex = startIndex;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T RemoveFirst()
		{
			if (CycledCount == 0)
			{
				throw new InvalidOperationException("List is empty.");
			}

			var index = MathUtils.FastMod(HeadIndex, CycleCapacity);
			CycledCount--;
			return Data[index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T RemoveLast()
		{
			if (CycledCount == 0)
			{
				throw new InvalidOperationException("List is empty.");
			}

			TailIndex--;
			var index = MathUtils.FastMod(TailIndex, CycleCapacity);
			CycledCount--;
			return Data[index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void TrimExcess()
		{
			var adjustedCycledCount = MathUtils.NextPowerOf2(CycledCount);
			if (adjustedCycledCount < CycleCapacity)
			{
				Resize(adjustedCycledCount);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void EnsureCapacity(int capcity)
		{
			if (CycleCapacity < capcity)
			{
				Resize(MathUtils.NextPowerOf2(capcity + 1));
			}
		}
		
		private void Resize(int newCapacity)
		{
			var newData = new T[newCapacity];
			for (var i = HeadIndex; i < TailIndex; i++)
			{
				newData[MathUtils.FastMod(i, newCapacity)] = Data[MathUtils.FastMod(i, CycleCapacity)];
			}
			Data = newData;
			CycleCapacity = newCapacity;
		}
	}
}
