﻿using System;

namespace Massive.Netcode
{
	public class ResimulationLoop
	{
		private readonly IMassive _massive;
		private readonly ISimulationSystem _simulationSystem;
		private readonly IInputPrediction _inputPrediction;
		private readonly ChangeTracker _changeTracker;
		private readonly int _saveEachNthTick;

		public ResimulationLoop(IMassive massive, ISimulationSystem simulationSystem, IInputPrediction inputPrediction, ChangeTracker changeTracker, int saveEachNthTick = 5)
		{
			_massive = massive;
			_simulationSystem = simulationSystem;
			_inputPrediction = inputPrediction;
			_changeTracker = changeTracker;
			_saveEachNthTick = saveEachNthTick;
		}

		public int CurrentTick { get; private set; }

		public void FastForwardToTick(int targetTick)
		{
			if (targetTick < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(targetTick), "Target frame should not be negative!");
			}

			int earliestTick = Math.Min(targetTick, _changeTracker.EarliestChangedTick);
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
				_simulationSystem.Update(CurrentTick);
				CurrentTick += 1;

				if (CurrentTick % _saveEachNthTick == 0)
				{
					_massive.SaveFrame();
				}
			}

			_changeTracker.ConfirmChangesUpTo(targetTick);
		}
	}
}
