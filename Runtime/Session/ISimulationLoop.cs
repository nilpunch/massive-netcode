namespace Massive.Netcode
{
	public interface ISimulationLoop
	{
		int CurrentTick { get; }

		void Reset(int startTick);

		void FastForwardToTick(int targetTick);
	}
}
