namespace Massive.Netcode
{
	public class FadeOutInputBuffer<TInput> : InputBuffer<TInput> where TInput : IFadeOutInput<TInput>
	{
		private readonly FadeOutConfig _fadeOutConfig;

		public FadeOutInputBuffer(int startTick, int bufferSize, FadeOutConfig? fadeOutConfig = default) : base(startTick, bufferSize)
		{
			_fadeOutConfig = fadeOutConfig ?? new FadeOutConfig(30, 60);
		}

		protected override TInput Predict(TInput input, int ticksPassed)
		{
			return input.FadeOut(ticksPassed, in _fadeOutConfig);
		}
	}
}
