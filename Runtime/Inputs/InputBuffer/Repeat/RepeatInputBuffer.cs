namespace Massive.Netcode
{
	public class RepeatInputBuffer<TInput> : InputBuffer<TInput>
	{
		public RepeatInputBuffer(int startTick, int bufferSize) : base(startTick, bufferSize)
		{
		}

		public override TInput GetPredicted(int tick)
		{
			var input = GetInput(tick);
			return input.LastActualInput;
		}
	}
}
