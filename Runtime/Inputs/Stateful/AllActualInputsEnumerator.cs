using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace Massive.Netcode
{
	[Il2CppSetOption(Option.NullChecks, false)]
	[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
	[Il2CppSetOption(Option.DivideByZeroChecks, false)]
	public struct ActualInputEnumerator<T>
	{
		private readonly AllInputs<T> _allInputs;
		private int _index;

		public ActualInputEnumerator(AllInputs<T> allInputs)
		{
			_allInputs = allInputs;
			_index = _allInputs.MaxChannels;
		}

		public (int channel, Input<T> input) Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => (_index, _allInputs.Inputs[_index]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			while (--_index >= 0 && !_allInputs.Inputs[_index].IsActual())
			{
			}

			return _index >= 0;
		}
	}
}
