using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace Massive.Netcode
{
	[Il2CppSetOption(Option.NullChecks, false)]
	[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
	public struct FreshInputsEnumerator<T> where T : IInput
	{
		private AllInputs<T> _allInputs;
		private int _index;

		public FreshInputsEnumerator(AllInputs<T> allInputs)
		{
			_allInputs = allInputs;
			_index = -1;
		}

		public (int Channel, Input<T> Input) Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => (_index, _allInputs.Inputs[_index]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			while (++_index < _allInputs.UsedChannels && !_allInputs.Inputs[_index].IsFresh)
			{
			}

			return _index < _allInputs.UsedChannels;
		}
	}
}
