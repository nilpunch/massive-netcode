namespace Massive.Netcode
{
	public class Client
	{
		public Session Session { get; }

		public TickSync TickSync { get; }

		public void Connect()
		{
			
		}

		public void Disconnect()
		{
			
		}

		public void Update()
		{
			var targetTick = TickSync.RemoteSimulationTick + TickSync.OneWayDelay;

			var predictionLimit = TickSync.LastReceivedTick + Session.Config.RollbackTicksCapacity;

			var clampedTargetTick = MathUtils.Min(targetTick, predictionLimit);

			Session.Loop.FastForwardToTick(clampedTargetTick);
		}
	}
}