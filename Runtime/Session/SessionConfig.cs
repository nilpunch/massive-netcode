namespace Massive.Netcode
{
	public class SessionConfig
	{
		public readonly int TickRate = 60;

		public readonly int SaveEachNthTick = 5;

		public readonly MassiveWorldConfig WorldConfig = new MassiveWorldConfig(framesCapacity: 26);

		public int FramesCapacity => WorldConfig.FramesCapacity;

		public int RollbackTicksCapacity => (FramesCapacity - 1) * SaveEachNthTick;

		public SessionConfig(int? tickRate = default, int? saveEachNthTick = default, MassiveWorldConfig worldConfig = default)
		{
			TickRate = tickRate ?? TickRate;
			SaveEachNthTick = saveEachNthTick ?? SaveEachNthTick;
			WorldConfig = worldConfig ?? WorldConfig;
		}
	}
}
