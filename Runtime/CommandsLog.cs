namespace Massive.Netcode
{
	public class CommandsLog
	{
		public int EarliestConfirmedFrame { get; private set; }

		public void NotifyChange(int frame)
		{
			if (EarliestConfirmedFrame > frame)
			{
				EarliestConfirmedFrame = frame;
			}
		}

		public void ConfirmObservation(int frame)
		{
			if (EarliestConfirmedFrame < frame)
			{
				EarliestConfirmedFrame = frame;
			}
		}
	}
}
