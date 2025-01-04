namespace Massive.Netcode
{
	public class PredictionInputBuffer<TInput> : InputBuffer<TInput>
	{
		private readonly InputPrediction<TInput> _inputPrediction;
		
		public PredictionInputBuffer(int startTick, int bufferSize, InputPrediction<TInput> inputPrediction = null) : base(startTick, bufferSize)
		{
			_inputPrediction = inputPrediction ?? RepeatPrediction;
		}

		public static TInput RepeatPrediction(TInput input, int ticksPassed) => input;

		public static TInput ResetPrediction(TInput input, int ticksPassed) => default;

		protected override TInput Predict(TInput input, int ticksPassed)
		{
			return _inputPrediction(input, ticksPassed);
		}
	}
}
