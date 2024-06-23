namespace Massive.Netcode
{
	public class FrameChangeLog
	{
		public int EarliestChangedFrame { get; private set; }

		public void NotifyFrameChange(int frame)
		{
			if (EarliestChangedFrame > frame)
			{
				EarliestChangedFrame = frame;
			}
		}

		public void ConfirmObservationUpTo(int frame)
		{
			if (EarliestChangedFrame < frame)
			{
				EarliestChangedFrame = frame;
			}
		}
	}
}
