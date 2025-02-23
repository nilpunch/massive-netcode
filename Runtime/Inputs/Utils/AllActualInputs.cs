using System.Runtime.CompilerServices;

namespace Massive.Netcode
{
	public readonly struct AllActualInputs<T>
	{
		public readonly AllInputs<T> AllInputs;

		public AllActualInputs(AllInputs<T> allInputs)
		{
			AllInputs = allInputs;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ActualInputEnumerator<T> GetEnumerator()
		{
			return new ActualInputEnumerator<T>(AllInputs);
		}
	}
}
