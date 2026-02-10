using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace Massive.Netcode
{
	[Il2CppSetOption(Option.NullChecks, false)]
	[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
	public struct AllEventsEnumerator<T> where T : IEvent
	{
		private readonly AllEvents<T> _allEvents;
		private ulong[] _allBits;
		private int _allBitsLength;
		private int _bitsIndex;

		private readonly byte[] _deBruijn;

		private ulong _bits;
		private int _bitsOffset;
		private int _bit;

		public AllEventsEnumerator(AllEvents<T> allEvents)
		{
			_allEvents = allEvents;
			_allBitsLength = allEvents.MaskLength;
			_allBits = allEvents.AllMask;

			_deBruijn = MathUtils.DeBruijn;

			_bitsIndex = -1;
			_bitsOffset = default;
			_bits = default;
			_bit = default;

			while (++_bitsIndex < _allBitsLength)
			{
				if (_allBits[_bitsIndex] != 0UL)
				{
					_bits = _allBits[_bitsIndex];
					_bitsOffset = _bitsIndex << 6;
					return;
				}
			}
		}

		public Event<T> Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _allEvents.Events[_bit + _bitsOffset];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			if (_bits != 0UL)
			{
				_bit = _deBruijn[(int)(((_bits & (ulong)-(long)_bits) * 0x37E84A99DAE458FUL) >> 58)];
				_bits &= _bits - 1UL;
				return true;
			}

			while (++_bitsIndex < _allBitsLength)
			{
				if (_allBits[_bitsIndex] != 0UL)
				{
					_bits = _allBits[_bitsIndex];
					_bitsOffset = _bitsIndex << 6;
					_bit = _deBruijn[(int)(((_bits & (ulong)-(long)_bits) * 0x37E84A99DAE458FUL) >> 58)];
					_bits &= _bits - 1UL;
					return true;
				}
			}

			return false;
		}
	}
}
