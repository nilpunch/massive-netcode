namespace Massive.Netcode
{
	public class RepeatInputBuffer<TInput> : InputBuffer<TInput>
	{
		public RepeatInputBuffer(int startTick, int bufferSize) : base(startTick, bufferSize)
		{
		}

		protected override TInput Predict(TInput input, int ticksPassed)
		{
			return input;
		}
	}
}
