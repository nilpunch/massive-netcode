namespace Massive.Netcode
{
	public class SessionConfig
	{
		public readonly int Framerate = 60;

		public readonly int SaveEachNthTick = 5;

		public readonly int StartTick = 0;

		public readonly MassiveWorldConfig WorldConfig = new MassiveWorldConfig();

		public SessionConfig(int? framerate = default, int? saveEachNthTick = default, int? startTick = default, MassiveWorldConfig worldConfig = default)
		{
			Framerate = framerate ?? Framerate;
			SaveEachNthTick = saveEachNthTick ?? SaveEachNthTick;
			StartTick = startTick ?? StartTick;
			WorldConfig = worldConfig ?? WorldConfig;
		}
	}
}
