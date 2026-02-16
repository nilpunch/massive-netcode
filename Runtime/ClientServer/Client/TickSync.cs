using System;

namespace Massive.Netcode
{
	public class TickSync
	{
		public int TickRate { get; }
		public int MaxRollbackTicks { get; }
		public int SafetyBufferTicks { get; }

		public int ApprovedSimulationTick { get; private set; }

		public double TimeSyncServerTime { get; private set; }
		public double TimeSyncClientTime { get; private set; }

		public int PredictionLeadTicks { get; private set; }

		public TickSync(int tickRate, int maxRollbackTicks, int safetyBufferTicks = 2)
		{
			TickRate = tickRate;
			MaxRollbackTicks = maxRollbackTicks;
			SafetyBufferTicks = safetyBufferTicks;

			PredictionLeadTicks = safetyBufferTicks;
		}

		private int MaxPredictionTick => ApprovedSimulationTick + PredictionLeadTicks;

		/// <summary>
		/// Computes the client simulation target tick.
		/// Clamped to the allowed rollback window.
		/// </summary>
		public int CalculateTargetTick(double clientTime)
		{
			var targetTick = EstimateServerTick(clientTime) + PredictionLeadTicks;
			return MathUtils.Max(ApprovedSimulationTick, MathUtils.Min(targetTick, MaxPredictionTick));
		}

		/// <summary>
		/// Estimates the current server tick from the time-sync anchor.
		/// </summary>
		public int EstimateServerTick(double clientTime)
		{
			var elapsed = clientTime - TimeSyncClientTime;
			var serverTime = TimeSyncServerTime + elapsed;
			return (int)Math.Floor(serverTime * TickRate);
		}

		/// <summary>
		/// Updates the server clock time-sync anchor.<br/>
		/// Call when receiving time syncing packet.
		/// </summary>
		public void UpdateTimeSync(double serverTime, double clientTime)
		{
			if (serverTime <= TimeSyncServerTime)
			{
				return;
			}

			TimeSyncServerTime = serverTime;
			TimeSyncClientTime = clientTime;
		}

		/// <summary>
		/// Marks this simulation tick as approved for extending rollback window.<br/>
		/// Call when all input packets received up to this simulation tick.
		/// </summary>
		public void ApproveSimulationTick(int tick)
		{
			if (tick <= ApprovedSimulationTick)
			{
				return;
			}

			ApprovedSimulationTick = tick;
		}

		/// <summary>
		/// Updates prediction lead based on RTT.
		/// </summary>
		public void UpdateRTT(double rttEstimate)
		{
			if (rttEstimate > 0)
			{
				var oneWayTicks = (int)Math.Ceiling((rttEstimate + 0.5f / TickRate) * 0.5 * TickRate);
				PredictionLeadTicks = MathUtils.Min(
					oneWayTicks + SafetyBufferTicks,
					MaxRollbackTicks);
			}
		}

		/// <summary>
		/// Returns normalized interpolation within the current tick [0..1].
		/// </summary>
		public float CalculateInterpolation(double clientTime)
		{
			var serverTickStartTime = EstimateServerTick(clientTime) / TickRate;

			var simulationDeltaTime = 1f / TickRate;

			var interpolation = (clientTime - serverTickStartTime) / simulationDeltaTime;

			var interpolation01 = interpolation < 0f ? 0f : interpolation > 1f ? 1f : (float)interpolation;

			return interpolation01;
		}

		public void Reset()
		{
			ApprovedSimulationTick = 0;
			TimeSyncServerTime = 0f;
			TimeSyncClientTime = 0f;
			PredictionLeadTicks = SafetyBufferTicks;
		}
	}
}
