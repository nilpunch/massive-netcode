namespace Massive.Netcode
{
	public class SimulationConfig
	{
		public readonly int Framerate = 60;

		public readonly int SaveEachNthTick = 5;

		public readonly int AdditionalInputBufferSize = 1;

		public readonly MassiveRegistryConfig MassiveRegistryConfig = new MassiveRegistryConfig();

		public SimulationConfig(int? framerate = default, int? saveEachNthTick = default,
			int? additionalInputBufferSize = default, MassiveRegistryConfig massiveRegistryConfig = default)
		{
			Framerate = framerate ?? Framerate;
			SaveEachNthTick = saveEachNthTick ?? SaveEachNthTick;
			AdditionalInputBufferSize = additionalInputBufferSize ?? AdditionalInputBufferSize;
			MassiveRegistryConfig = massiveRegistryConfig ?? MassiveRegistryConfig;
		}
	}
}
