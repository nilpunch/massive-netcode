using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace Massive.Netcode
{
	[Il2CppSetOption(Option.NullChecks, false)]
	[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
	[Il2CppSetOption(Option.DivideByZeroChecks, false)]
	public struct PackedInputEnumerator<T> : IDisposable
	{
		private readonly DataSet<InputBuffer<T>> _inputSet;
		private readonly int _tick;
		private readonly Packing _originalPacking;
		private int _index;

		public PackedInputEnumerator(DataSet<InputBuffer<T>> inputSet, int tick, Packing packingWhenIterating = Packing.WithHoles)
		{
			_inputSet = inputSet;
			_tick = tick;
			_originalPacking = _inputSet.ExchangeToStricterPacking(packingWhenIterating);
			_index = _inputSet.Count;
		}

		public (int client, T input) Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				var client = _inputSet.Packed[_index];
				return (client, _inputSet.Data[_index].GetInput(_tick));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			if (--_index > _inputSet.Count)
			{
				_index = _inputSet.Count - 1;
			}

			while (_index >= 0 && _inputSet.Packed[_index] < 0)
			{
				--_index;
			}

			return _index >= 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			_inputSet.ExchangePacking(_originalPacking);
		}
	}
}
