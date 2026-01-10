namespace Massive.Netcode
{
	public readonly struct Input<TInput>
	{
		public readonly TInput LastActualInput;
		public readonly int TicksPassed;

		public Input(TInput lastActualInput, int ticksPassed)
		{
			LastActualInput = lastActualInput;
			TicksPassed = ticksPassed;
		}

		public static readonly Input<TInput> Inactual = new Input<TInput>(Default<TInput>.Value, int.MaxValue);
	}
}
