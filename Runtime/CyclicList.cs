using System;
using System.Runtime.CompilerServices;

namespace Massive.Netcode
{
	public class CyclicList<T>
	{
		private readonly T[] _data;

		public int CycledCount { get; private set; }

		public int TailIndex { get; private set; }

		public int HeadIndex => TailIndex - CycledCount;

		public int CycleCapacity => _data.Length;

		public CyclicList(int size, int startFrom = 0)
		{
			_data = new T[size];
			TailIndex = startFrom;
		}

		public ref T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref _data[index - HeadIndex];
		}

		public void Add(T data)
		{
			_data[TailIndex % CycleCapacity] = data;
			TailIndex += 1;
			CycledCount = Math.Min(CycledCount + 1, CycleCapacity);
		}
	}
}
