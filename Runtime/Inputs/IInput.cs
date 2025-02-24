namespace Massive.Netcode
{
	public interface IInput
	{
		void PopulateUpTo(int tick);
		void DiscardUpTo(int tick);
		void Reevaluate();
	}
}
