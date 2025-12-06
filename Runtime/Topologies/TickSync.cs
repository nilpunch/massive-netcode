namespace Massive.Netcode
{
	public class TickSync
	{
		public int LastReceivedTick { get; set; }

		/// <summary>
		/// Synchornized clock.
		/// </summary>
		public int RemoteSimulationTick { get; set; }

		/// <summary>
		/// Should be calculated as latency + some offset.
		/// </summary>
		public int OneWayDelay { get; set; }
	}
}