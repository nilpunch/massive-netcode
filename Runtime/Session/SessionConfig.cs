namespace Massive.Netcode
{
	public class SessionConfig
	{
		public readonly int TickRate = 60;

		public readonly int SaveEachNthTick = 5;

		public readonly int StartTick = 0;

		public readonly MassiveWorldConfig WorldConfig = new MassiveWorldConfig();

		public int FramesCapacity => WorldConfig.FramesCapacity;

		public int RollbackTicksCapacity => FramesCapacity * SaveEachNthTick;

		public SessionConfig(int? tickRate = default, int? saveEachNthTick = default, int? startTick = default, MassiveWorldConfig worldConfig = default)
		{
			TickRate = tickRate ?? TickRate;
			SaveEachNthTick = saveEachNthTick ?? SaveEachNthTick;
			StartTick = startTick ?? StartTick;
			WorldConfig = worldConfig ?? WorldConfig;
		}
	}
}
