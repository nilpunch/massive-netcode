namespace Massive.Netcode
{
	public class ChangeTracker
	{
		public int EarliestChangedTick { get; private set; }

		public void NotifyChange(int tick)
		{
			if (EarliestChangedTick > tick)
			{
				EarliestChangedTick = tick;
			}
		}

		public void ConfirmChangesUpTo(int tick)
		{
			if (EarliestChangedTick < tick)
			{
				EarliestChangedTick = tick;
			}
		}
	}
}
