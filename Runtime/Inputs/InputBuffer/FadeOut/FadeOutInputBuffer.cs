namespace Massive.Netcode
{
	public class FadeOutInputBuffer<TInput> : InputBuffer<TInput> where TInput : IFadeOutInput<TInput>
	{
		public FadeOutInputBuffer(int startTick, int bufferSize) : base(startTick, bufferSize)
		{
		}

		protected override TInput Predict(TInput input, int ticksPassed)
		{
			return input.FadeOut(ticksPassed);
		}
	}
}
