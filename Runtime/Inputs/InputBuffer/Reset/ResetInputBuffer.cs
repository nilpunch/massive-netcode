namespace Massive.Netcode
{
	public class ResetInputBuffer<TInput> : InputBuffer<TInput>
	{
		public ResetInputBuffer(int startTick, int bufferSize) : base(startTick, bufferSize)
		{
		}

		public override TInput GetPredicted(int tick)
		{
			var input = GetInput(tick);
			return input.TicksPassed == 0
				? input.LastActualInput
				: default;
		}
	}
}
