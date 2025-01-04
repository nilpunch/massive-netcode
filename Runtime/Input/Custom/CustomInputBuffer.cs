namespace Massive.Netcode
{
	public class CustomInputBuffer<TInput> : InputBuffer<TInput> where TInput : ICustomInput<TInput>
	{
		public CustomInputBuffer(int startTick, int bufferSize) : base(startTick, bufferSize)
		{
		}

		protected override TInput Predict(TInput input, int ticksPassed)
		{
			return input.Predict(ticksPassed);
		}
	}
}
