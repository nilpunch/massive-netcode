using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace Massive.Netcode
{
	[Il2CppSetOption(Option.NullChecks, false)]
	[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
	public struct ActualInputEnumerator<T>
	{
		private AllInputs<T> _allInputs;
		private int _index;

		public ActualInputEnumerator(AllInputs<T> allInputs)
		{
			_allInputs = allInputs;
			_index = _allInputs.UsedChannels;
		}

		public (int Channel, Input<T> Input) Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => (_index, _allInputs.Inputs[_index].GetInputAt(_allInputs.Tick));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			while (++_index < _allInputs.UsedChannels && !_allInputs.Inputs[_index].IsActualAt(_allInputs.Tick))
			{
			}

			return _index < _allInputs.UsedChannels;
		}
	}
}
