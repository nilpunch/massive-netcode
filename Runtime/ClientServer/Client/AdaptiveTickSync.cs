namespace Massive.Netcode
{
	public class AdaptiveTickSync : TickSync
	{
		public AdaptiveTickSync(int tickRate, int maxRollbackTicks, int safetyBufferTicks = 2) : base(tickRate, maxRollbackTicks, safetyBufferTicks)
		{
		}

		public override int CalculateTargetTick(double clientTime)
		{
			return base.CalculateTargetTick(clientTime);
		}
	}
}
