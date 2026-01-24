using System.Runtime.CompilerServices;

namespace Massive.Netcode
{
	public readonly struct FreshInputsEnumerable<T> where T : IInput
	{
		public readonly AllInputs<T> AllInputs;

		public FreshInputsEnumerable(AllInputs<T> allInputs)
		{
			AllInputs = allInputs;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FreshInputsEnumerator<T> GetEnumerator()
		{
			return new FreshInputsEnumerator<T>(AllInputs);
		}
	}
}
