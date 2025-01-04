namespace Massive.Netcode
{
	public interface IInputBuffer
	{
		void ResetInputs(int startTick);
	}

	public interface IInputPrediction
	{
		void PopulateInputsUpTo(int tick);
	}
}