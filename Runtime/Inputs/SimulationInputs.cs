namespace Massive.Netcode
{
	public class SimulationInputs : ClientInputs
	{
		public SimulationTime Time { get; }

		public SimulationInputs(SimulationTime time, int inputBufferSize = 120, int startTick = 0, RegistryConfig registryConfig = null)
			: base(inputBufferSize, startTick, registryConfig)
		{
			Time = time;
		}

		public T GetMasterInput<T>()
		{
			return GetMasterInputAt<T>(Time.Tick);
		}

		public T GetInput<T>(int client)
		{
			return GetInputAt<T>(client, Time.Tick);
		}
	}
}
