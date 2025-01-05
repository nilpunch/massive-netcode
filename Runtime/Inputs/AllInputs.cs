namespace Massive.Netcode
{
	public struct AllInputs<T>
	{
		public readonly DataSet<InputBuffer<T>> Set;
		public readonly int Tick;

		public AllInputs(DataSet<InputBuffer<T>> set, int tick)
		{
			Set = set;
			Tick = tick;
		}

		public T GetInput(int client)
		{
			return Set.Get(client).GetInput(Tick);
		}
	}
}
