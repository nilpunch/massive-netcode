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

		public T GetGlobal<T>()
		{
			return GetGlobalAt<T>(Time.Tick);
		}

		public T Get<T>(int client)
		{
			return GetAt<T>(Time.Tick, client);
		}

		public AllInputs<T> GetAll<T>()
		{
			return GetAllAt<T>(Time.Tick);
		}
	}
}
