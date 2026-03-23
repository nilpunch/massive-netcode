using System;

namespace Massive.Netcode
{
	public class AdaptiveTickSync : TickSync
	{
		private const double RttAlpha = 0.125;

		/// <summary>
		/// This is time to close 50% of the gap. 0.5 = snappy, 2.0 = sluggish.
		/// </summary>
		public double SmoothingTime { get; }

		public double SmoothedRtt { get; private set; }
		public double SmoothedLeadTime { get; private set; }

		private double _lastClientTime = -1;

		public AdaptiveTickSync(
			int safetyBufferTicks = 2,
			double smoothingTime = 0.5)
			: base(safetyBufferTicks)
		{
			SmoothingTime = smoothingTime;
		}

		public override int CalculateTargetTick(double clientTime)
		{
			if (_lastClientTime < 0)
			{
				SmoothedLeadTime = (double)PredictionLeadTicks / TickRate;
			}
			else
			{
				var dt = clientTime - _lastClientTime;
				
				var diff = (double)PredictionLeadTicks / TickRate - SmoothedLeadTime;
				var lerpRate = 1.0 - Math.Pow(0.5, dt / SmoothingTime);
				SmoothedLeadTime += diff * lerpRate;
			}

			_lastClientTime = clientTime;

			var serverTime = EstimateServerTime(clientTime) + SmoothedLeadTime;
			var targetTick = (int)Math.Floor(serverTime * TickRate);
			return MathUtils.Max(MinPredictionTick, MathUtils.Min(targetTick, MaxPredictionTick));
		}

		public override void UpdateRTT(double rttSample)
		{
			if (rttSample <= 0)
			{
				return;
			}

			if (SmoothedRtt <= 0)
			{
				SmoothedRtt = rttSample;
			}
			else if (rttSample > SmoothedRtt)
			{
				SmoothedRtt = rttSample;
			}
			else
			{
				SmoothedRtt = SmoothedRtt * (1 - RttAlpha) + rttSample * RttAlpha;
			}

			var oneWayTicks = (int)Math.Ceiling(SmoothedRtt * 0.5 * TickRate);
			PredictionLeadTicks = MathUtils.Min(oneWayTicks + SafetyBufferTicks, MaxRollbackTicks);
		}

		public override void Reset()
		{
			base.Reset();
			SmoothedRtt = 0;
			_lastClientTime = -1;
		}
	}
}
