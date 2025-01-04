namespace Massive.Netcode
{
	public class TickChangeLog
	{
		public int EarliestChangedTick { get; private set; }

		public void NotifyChangeAt(int tick)
		{
			if (EarliestChangedTick > tick)
			{
				EarliestChangedTick = tick;
			}
		}

		public void ConfirmUpTo(int tick)
		{
			if (EarliestChangedTick < tick)
			{
				EarliestChangedTick = tick;
			}
		}
	}
}
