using System;

namespace Massive.Netcode
{
	public class SimulationLoop
	{
		private readonly IMassive _massive;
		private readonly ISimulation _simulation;
		private readonly IInputPrediction _inputPrediction;
		private readonly TickChangeLog _tickChangeLog;
		private readonly int _saveEachNthTick;

		public SimulationLoop(IMassive massive, ISimulation simulation, IInputPrediction inputPrediction, TickChangeLog tickChangeLog, int saveEachNthTick = 5)
		{
			_massive = massive;
			_simulation = simulation;
			_inputPrediction = inputPrediction;
			_tickChangeLog = tickChangeLog;
			_saveEachNthTick = saveEachNthTick;
		}

		public int CurrentTick { get; private set; }

		public void FastForwardToTick(int targetTick)
		{
			if (targetTick < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(targetTick), "Target frame should not be negative!");
			}

			int earliestTick = Math.Min(targetTick, _tickChangeLog.EarliestChangedTick);
			int ticksToRollback = Math.Max(CurrentTick - earliestTick, 0);

			int currentFrame = CurrentTick / _saveEachNthTick;
			int targetFrame = (CurrentTick - ticksToRollback) / _saveEachNthTick;
			int framesToRollback = currentFrame - targetFrame;

			if (framesToRollback > _massive.CanRollbackFrames)
			{
				throw new InvalidOperationException("Can't rollback this far!");
			}

			_massive.Rollback(framesToRollback);
			CurrentTick = (currentFrame - framesToRollback) * _saveEachNthTick;

			_inputPrediction.PopulateInputsUpTo(targetTick);

			while (CurrentTick < targetTick)
			{
				_simulation.Update(CurrentTick);
				CurrentTick += 1;

				if (CurrentTick % _saveEachNthTick == 0)
				{
					_massive.SaveFrame();
				}
			}

			_tickChangeLog.ConfirmObservationUpTo(targetTick);
		}
	}
}
