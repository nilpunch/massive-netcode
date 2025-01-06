using System;

namespace Massive.Netcode.Samples
{
	// Smooth input prediction.
	// Note: Floats are used here for simplicity. Replace them with deterministic types in real usage.
	public struct PlayerMovingInput : IFadeOutInput<PlayerMovingInput>
	{
		public float MagnitudeX;
		public float MagnitudeY;

		public PlayerMovingInput FadeOut(int ticksPassed, in FadeOutConfig config)
		{
			float fadeOutPercent = Math.Clamp((ticksPassed - config.StartDecayTick) / (float)config.DecayDurationTicks, 0f, 1f);
			float modifier = 1f - fadeOutPercent;
			return new PlayerMovingInput()
			{
				MagnitudeX = MagnitudeX * modifier,
				MagnitudeY = MagnitudeY * modifier
			};
		}
	}
}
