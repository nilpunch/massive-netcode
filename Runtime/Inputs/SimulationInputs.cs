namespace Massive.Netcode
{
	public class SimulationInputs
	{
		public InputRegistry InputRegistry { get; }

		public SimulationTime SimulationTime { get; }

		public SimulationInputs(InputRegistry inputRegistry, SimulationTime simulationTime)
		{
			InputRegistry = inputRegistry;
			SimulationTime = simulationTime;
		}
	}
}
