﻿using System;

namespace Massive.Netcode
{
	public class Simulation
	{
		private readonly IMassive _massive;
		private readonly ISystem _system;
		private readonly ICommandBuffer _commandBuffer;
		private readonly FrameChangeLog _frameChangeLog;

		public Simulation(IMassive massive, ISystem system, ICommandBuffer commandBuffer, FrameChangeLog frameChangeLog)
		{
			_massive = massive;
			_system = system;
			_commandBuffer = commandBuffer;
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

			_commandBuffer.PopulateCommandsUpTo(targetFrame);

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
