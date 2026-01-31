namespace Massive.Netcode
{
	public interface IInputs
	{
		void PopulateUpTo(int tick);
		void DiscardUpTo(int tick);
		void Reevaluate();
	}
}
