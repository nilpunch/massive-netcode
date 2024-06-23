using System;

namespace Massive.Netcode
{
	public class Simulation
	{
		private readonly IMassive _massive;
		private readonly ISystem _system;
		private readonly ICommandTimeline _commandTimeline;
		private readonly FrameChangeLog _frameChangeLog;

		public Simulation(IMassive massive, ISystem system, ICommandTimeline commandTimeline, FrameChangeLog frameChangeLog)
		{
			_massive = massive;
			_system = system;
			_commandTimeline = commandTimeline;
			_frameChangeLog = frameChangeLog;
		}

		private int CurrentFrame { get; set; }

		public void FastForwardToFrame(int targetFrame)
		{
			if (targetFrame < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(targetFrame), "Target frame should not be negative!");
			}

			int earliestFrame = Math.Min(targetFrame, _frameChangeLog.EarliestChangedFrame);
			int framesToRollback = Math.Max(CurrentFrame - earliestFrame, 0);

			_massive.Rollback(framesToRollback);
			CurrentFrame -= framesToRollback;

			_commandTimeline.PopulateCommandsUpTo(targetFrame);

			while (CurrentFrame < targetFrame)
			{
				_system.StepForward();
				_massive.SaveFrame();
				CurrentFrame += 1;
			}

			_frameChangeLog.ConfirmObservationUpTo(targetFrame);
		}
	}
}
