namespace Massive.Netcode
{
	public interface IInputBuffer
	{
		void PopulateInputsUpTo(int tick);
		void ForgetInputsUpTo(int tick);
	}
}
