using System;

namespace Massive.Netcode
{
	public class ResimulationLoop
	{
		private readonly IMassive _massive;
		private readonly ISimulation _simulation;
		private readonly IInput _input;
		private readonly ChangeTracker _changeTracker;
		private readonly int _saveEachNthTick;

		public ResimulationLoop(IMassive massive, ISimulation simulation, IInput input, ChangeTracker changeTracker, int saveEachNthTick = 5)
		{
			_massive = massive;
			_simulation = simulation;
			_input = input;
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

			var earliestTick = Math.Min(targetTick, _changeTracker.EarliestChangedTick);
			var ticksToRollback = Math.Max(CurrentTick - earliestTick, 0);

			var currentFrame = CurrentTick / _saveEachNthTick;
			var targetFrame = (CurrentTick - ticksToRollback) / _saveEachNthTick;
			var framesToRollback = currentFrame - targetFrame;

			if (framesToRollback > _massive.CanRollbackFrames)
			{
				throw new InvalidOperationException("Can't rollback this far!");
			}

			_massive.Rollback(framesToRollback);
			CurrentTick = (currentFrame - framesToRollback) * _saveEachNthTick;

			_input.ReevaluateInputs();
			_input.PopulateInputsUpTo(targetTick);

			while (CurrentTick < targetTick)
			{
				_simulation.Update(CurrentTick);
				CurrentTick += 1;

				if (CurrentTick % _saveEachNthTick == 0)
				{
					_massive.SaveFrame();
				}
			}

			_input.DiscardInputsUpTo(targetTick - (_massive.CanRollbackFrames + 1) * _saveEachNthTick);

			_changeTracker.ConfirmChangesUpTo(targetTick);
		}
	}
}
