namespace Massive.Netcode
{
	public interface IInput
	{
		void PopulateInputsUpTo(int tick);
		void DiscardInputsUpTo(int tick);
		void ReevaluateInputs();
	}
}
