namespace Massive.Netcode
{
	public class ResetInputBuffer<TInput> : InputBuffer<TInput>
	{
		public ResetInputBuffer(int startTick, int bufferSize) : base(startTick, bufferSize)
		{
		}

		protected override TInput Predict(TInput input, int ticksPassed)
		{
			return default;
		}
	}
}
