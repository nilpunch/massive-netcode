using System;

namespace Massive.Netcode
{
	public class AdaptiveTickSync : TickSync
	{
		private const double RttAlpha = 0.125;

		/// <summary>
		/// This is time to close 50% of the gap. 0.5 = snappy, 2.0 = sluggish.
		/// </summary>
		public double TargetTickSmoothingTime { get; }

		public double SmoothedRtt { get; private set; }

		/// <summary>
		/// Current fractional simulation tick. Advances smoothly toward the RTT-derived target.
		/// </summary>
		public double SmoothedTargetTick { get; private set; }

		private double _lastClientTime = -1;

		public AdaptiveTickSync(
			int safetyBufferTicks = 2,
			double targetTickSmoothingTime = 0.5)
			: base(safetyBufferTicks)
		{
			TargetTickSmoothingTime = targetTickSmoothingTime;
		}

		public override int CalculateTargetTick(double clientTime)
		{
			var hardTargetTick = base.CalculateTargetTick(clientTime);

			if (_lastClientTime < 0 || SmoothedTargetTick <= 0)
			{
				SmoothedTargetTick = hardTargetTick;
				_lastClientTime = clientTime;
				return hardTargetTick;
			}

			var dt = clientTime - _lastClientTime;
			_lastClientTime = clientTime;

			var diff = hardTargetTick - SmoothedTargetTick;

			var lerpRate = 1.0 - Math.Pow(0.5f, dt / TargetTickSmoothingTime);
			SmoothedTargetTick += diff * lerpRate;

			SmoothedTargetTick = Math.Clamp(
				SmoothedTargetTick,
				MinPredictionTick,
				hardTargetTick);

			return (int)Math.Floor(SmoothedTargetTick);
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
			SmoothedTargetTick = 0;
			_lastClientTime = -1;
		}
	}
}
