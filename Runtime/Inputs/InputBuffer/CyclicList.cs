using System;
using System.Runtime.CompilerServices;

namespace Massive.Netcode
{
	public class CyclicList<T>
	{
		private readonly T[] _data;

		public int CycledCount { get; private set; }

		public int TailIndex { get; private set; }

		public int HeadIndex
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => TailIndex - CycledCount;
		}

		public int CycleCapacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _data.Length;
		}

		public CyclicList(int size, int startIndex = 0)
		{
			_data = new T[size];
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

				return ref _data[index - HeadIndex];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(T data)
		{
			_data[TailIndex % CycleCapacity] = data;
			TailIndex += 1;
			CycledCount = Math.Min(CycledCount + 1, CycleCapacity);
		}

		public void Reset(int startIndex)
		{
			CycledCount = 0;
			TailIndex = startIndex;
		}
	}
}
