using System.Runtime.CompilerServices;

namespace Massive.Netcode
{
	public readonly struct AllInputs<T>
	{
		public readonly DataSet<InputBuffer<T>> Set;
		public readonly int Tick;

		public AllInputs(DataSet<InputBuffer<T>> set, int tick)
		{
			Set = set;
			Tick = tick;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T GetInput(int client)
		{
			return Set.Get(client).GetPredicted(Tick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public PackedInputEnumerator<T> GetEnumerator()
		{
			return new PackedInputEnumerator<T>(Set, Tick);
		}
	}
}
