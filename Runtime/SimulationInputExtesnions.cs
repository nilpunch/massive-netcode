namespace Massive.Netcode
{
	public static class SimulationInputExtesnions
	{
		public static T GetGlobalInput<T>(this SimulationInput simulationInput)
		{
			return simulationInput.ClientInput.GetGlobalInput<T>(simulationInput.Time.Tick);
		}

		public static T GetInput<T>(this SimulationInput simulationInput, int client)
		{
			return simulationInput.ClientInput.GetInput<T>(client, simulationInput.Time.Tick);
		}
	}
}
