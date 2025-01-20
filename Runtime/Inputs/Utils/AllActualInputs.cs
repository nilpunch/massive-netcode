using System.Runtime.CompilerServices;

namespace Massive.Netcode
{
	public readonly struct AllActualInputs<T>
	{
		public readonly DataSet<InputBuffer<T>> Set;
		public readonly int Tick;

		public AllActualInputs(DataSet<InputBuffer<T>> set, int tick)
		{
			Set = set;
			Tick = tick;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ActualInputEnumerator<T> GetEnumerator()
		{
			return new ActualInputEnumerator<T>(Set, Tick);
		}
	}
}
