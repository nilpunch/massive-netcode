using System;

namespace Massive.Netcode
{
	public class TickSync
	{
		private readonly int _tickRate;
		private readonly int _maxRollbackTicks;
		private readonly int _safetyBufferTicks;

		public float LastUpdateClientTime { get; private set; }

		public int LastReceivedServerTick { get; private set; } = -1;

		public int EstimatedServerTick { get; private set; } = 0;

		public int PredictionOffset { get; private set; }

		public int TargetTick
		{
			get
			{
				if (LastReceivedServerTick < 0)
				{
					return 0;
				}

				var desiredTarget = EstimatedServerTick + PredictionOffset;
				return MathUtils.Min(desiredTarget, LastReceivedServerTick + _maxRollbackTicks);
			}
		}

		public TickSync(int tickRate, int maxRollbackTicks, int safetyBufferTicks = 2)
		{
			_tickRate = tickRate;
			_maxRollbackTicks = maxRollbackTicks;
			_safetyBufferTicks = safetyBufferTicks;
			PredictionOffset = safetyBufferTicks;
		}

		/// <summary>
		/// Update time estimation. Call every frame with current client time.
		/// </summary>
		public void Update(float clientTime)
		{
			UpdateEstimation(clientTime);
		}

		/// <summary>
		/// Call when receiving server tick.
		/// </summary>
		public void UpdateServerTick(int serverTick, float clientTime, float rttEstimate = 0)
		{
			if (serverTick <= LastReceivedServerTick)
			{
				return; // Old or duplicate update
			}

			LastReceivedServerTick = serverTick;
			LastUpdateClientTime = clientTime;

			if (rttEstimate > 0)
			{
				var oneWayDelaySeconds = rttEstimate * 0.5f;
				var oneWayDelayTicks = (int)MathF.Round(oneWayDelaySeconds * _tickRate);

				var desiredOffset = oneWayDelayTicks + _safetyBufferTicks;

				PredictionOffset = MathUtils.Min(desiredOffset, _maxRollbackTicks);
			}

			UpdateEstimation(clientTime);
		}

		public void Reset()
		{
			LastReceivedServerTick = -1;
			LastUpdateClientTime = -1f;
			EstimatedServerTick = 0;
			PredictionOffset = _safetyBufferTicks;
		}

		private void UpdateEstimation(float clientTime)
		{
			if (LastReceivedServerTick < 0)
			{
				EstimatedServerTick = 0;
				return;
			}

			var elapsed = clientTime - LastUpdateClientTime;
			var elapsedTicks = (int)MathF.Floor(elapsed * _tickRate);
			EstimatedServerTick = LastReceivedServerTick + MathUtils.Max(0, elapsedTicks);
		}
	}
}
