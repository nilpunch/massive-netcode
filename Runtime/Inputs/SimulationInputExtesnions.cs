namespace Massive.Netcode
{
	public static class SimulationInputExtesnions
	{
		public static T GetMasterInput<T>(this SimulationInputs simulationInputs)
		{
			return simulationInputs.InputRegistry.GetMasterInput<T>(simulationInputs.SimulationTime.Tick);
		}

		public static T GetInput<T>(this SimulationInputs simulationInputs, int client)
		{
			return simulationInputs.InputRegistry.GetInput<T>(client, simulationInputs.SimulationTime.Tick);
		}

		public static DataSet<InputBuffer<T>> GetAllInputs<T>(this SimulationInputs simulationInputs)
		{
			return simulationInputs.InputRegistry.GetAllInputs<T>();
		}
	}
}
