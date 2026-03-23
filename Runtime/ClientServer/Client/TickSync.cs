using System;

namespace Massive.Netcode
{
	public class TickSync
	{
		public int TickRate { get; set; } = 60;
		public int MaxRollbackTicks { get; set; } = 120;
		public int SafetyBufferTicks { get; }

		/// <summary>
		/// ApprovedSimulationTick + 1, basically this is the first tick that can be affected by prediction.
		/// Guards against simulating a tick the server has already approved.
		/// </summary>
		public int MinPredictionTick { get; private set; }

		public double TimeSyncServerTime { get; private set; }
		public double TimeSyncClientTime { get; private set; }

		public int PredictionLeadTicks { get; protected set; }

		public TickSync(int safetyBufferTicks = 2)
		{
			SafetyBufferTicks = safetyBufferTicks;
			PredictionLeadTicks = safetyBufferTicks;
		}

		public int MaxPredictionTick => MinPredictionTick + MaxRollbackTicks - 1;

		/// <summary>
		/// Computes the client simulation target tick.
		/// Clamped to the allowed rollback window.
		/// </summary>
		public virtual int CalculateTargetTick(double clientTime)
		{
			var targetTick = EstimateServerTick(clientTime) + PredictionLeadTicks;
			return MathUtils.Max(MinPredictionTick, MathUtils.Min(targetTick, MaxPredictionTick));
		}

		/// <summary>
		/// Estimates the current server tick from the time-sync anchor.
		/// </summary>
		public int EstimateServerTick(double clientTime)
		{
			var serverTime = EstimateServerTime(clientTime);
			return (int)Math.Floor(serverTime * TickRate);
		}

		/// <summary>
		/// Estimates the current server time from the time-sync anchor.
		/// </summary>
		public double EstimateServerTime(double clientTime)
		{
			var elapsed = clientTime - TimeSyncClientTime;
			return TimeSyncServerTime + elapsed;
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
			if (tick + 1 <= MinPredictionTick)
			{
				return;
			}

			MinPredictionTick = tick + 1;
		}

		/// <summary>
		/// Updates prediction lead based on RTT.
		/// </summary>
		public virtual void UpdateRTT(double rttEstimate)
		{
			if (rttEstimate > 0)
			{
				var oneWayTicks = (int)Math.Ceiling(rttEstimate * 0.5 * TickRate);
				PredictionLeadTicks = oneWayTicks + SafetyBufferTicks;
			}
		}

		/// <summary>
		/// Returns normalized interpolation within the current tick [0..1].
		/// </summary>
		public float CalculateInterpolation(double clientTime)
		{
			var serverTime = EstimateServerTime(clientTime);

			var simulationDeltaTime = 1f / TickRate;

			var serverTickStartTime = Math.Floor(serverTime / simulationDeltaTime) * simulationDeltaTime;

			var interpolation = (serverTime - serverTickStartTime) / simulationDeltaTime;

			return (float)Math.Clamp(interpolation, 0.0, 1.0);
		}

		public virtual void Reset()
		{
			MinPredictionTick = 0;
			TimeSyncServerTime = 0f;
			TimeSyncClientTime = 0f;
			PredictionLeadTicks = SafetyBufferTicks;
		}
	}
}
