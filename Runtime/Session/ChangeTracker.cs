using System.Runtime.CompilerServices;

namespace Massive.Netcode
{
	public class ChangeTracker
	{
		public int EarliestChangedTick { get; private set; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void NotifyChange(int tick)
		{
			NegativeArgumentException.ThrowIfNegative(tick);

			if (EarliestChangedTick > tick)
			{
				EarliestChangedTick = tick;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ConfirmChangesUpTo(int tick)
		{
			NegativeArgumentException.ThrowIfNegative(tick);

			if (EarliestChangedTick < tick)
			{
				EarliestChangedTick = tick;
			}
		}
	}
}
