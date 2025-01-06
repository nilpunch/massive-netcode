﻿namespace Massive.Netcode
{
	public struct FadeOutConfig
	{
		public int StartDecayTick;
		public int DecayDurationTicks;

		public FadeOutConfig(int startDecayTick, int decayDurationTicks)
		{
			StartDecayTick = startDecayTick;
			DecayDurationTicks = decayDurationTicks;
		}
	}
}
