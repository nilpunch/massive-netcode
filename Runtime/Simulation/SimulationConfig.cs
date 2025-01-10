namespace Massive.Netcode
{
	public class SimulationConfig
	{
		public readonly int Framerate = 60;

		public readonly int SaveEachNthTick = 5;

		public readonly int AdditionalInputBufferSize = 1;

		public readonly int StartTick = 0;

		public readonly MassiveRegistryConfig RegistryConfig = new MassiveRegistryConfig();

		public SimulationConfig(int? framerate = default, int? saveEachNthTick = default,
			int? additionalInputBufferSize = default, int? startTick = default, MassiveRegistryConfig registryConfig = default)
		{
			Framerate = framerate ?? Framerate;
			SaveEachNthTick = saveEachNthTick ?? SaveEachNthTick;
			AdditionalInputBufferSize = additionalInputBufferSize ?? AdditionalInputBufferSize;
			StartTick = startTick ?? StartTick;
			RegistryConfig = registryConfig ?? RegistryConfig;
		}
	}
}
