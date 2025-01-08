namespace Massive.Netcode
{
	public class FadeOutInputBuffer<TInput> : InputBuffer<TInput>, IFadeOutInputBuffer where TInput : IFadeOutInput<TInput>
	{
		public FadeOutConfig FadeOutConfig { get; set; }

		public FadeOutInputBuffer(int startTick, int bufferSize) : this(startTick, bufferSize, new FadeOutConfig(30, 60))
		{
		}

		public FadeOutInputBuffer(int startTick, int bufferSize, FadeOutConfig fadeOutConfig) : base(startTick, bufferSize)
		{
			FadeOutConfig = fadeOutConfig;
		}

		protected override TInput Predict(TInput input, int ticksPassed)
		{
			return input.FadeOut(ticksPassed, FadeOutConfig);
		}
	}
}
