using System;

namespace Massive.Netcode
{
	public class ResimulationLoop : ISimulationLoop
	{
		private readonly IMassive _massive;
		private readonly ISimulation _simulation;
		private readonly IInputs _inputs;
		private readonly ChangeTracker _changeTracker;
		private readonly int _saveEachNthTick;

		public ResimulationLoop(IMassive massive, ISimulation simulation, IInputs inputs, ChangeTracker changeTracker, int saveEachNthTick = 5)
		{
			_massive = massive;
			_simulation = simulation;
			_inputs = inputs;
			_changeTracker = changeTracker;
			_saveEachNthTick = saveEachNthTick;
		}

		public int CurrentTick { get; private set; }
		public int StartTick { get; private set; }

		public void Reset(int startTick)
		{
			StartTick = startTick;
			CurrentTick = startTick;
		}

		public void FastForwardToTick(int targetTick)
		{
			if (targetTick < StartTick)
			{
				throw new ArgumentOutOfRangeException(nameof(targetTick), "Target tick should not be negative.");
			}

			var earliestTick = Math.Min(targetTick, _changeTracker.EarliestChangedTick);
			var ticksToRollback = Math.Max(CurrentTick - earliestTick, 0);

			var currentFrame = CurrentTick / _saveEachNthTick;
			var targetFrame = Math.Max(StartTick, CurrentTick - ticksToRollback) / _saveEachNthTick;
			var framesToRollback = currentFrame - targetFrame;

			if (framesToRollback > _massive.CanRollbackFrames)
			{
				throw new InvalidOperationException($"Can't rollback this far. CanRollbackFrames: {_massive.CanRollbackFrames}, Actual: {framesToRollback}");
			}

			_massive.Rollback(framesToRollback);
			CurrentTick = Math.Max(StartTick, (currentFrame - framesToRollback) * _saveEachNthTick);

			_inputs.Reevaluate();
			_inputs.PopulateUpTo(targetTick);

			while (CurrentTick < targetTick)
			{
				_simulation.Update(CurrentTick);
				CurrentTick += 1;

				if (CurrentTick % _saveEachNthTick == 0)
				{
					_massive.SaveFrame();
				}
			}

			_inputs.DiscardUpTo(Math.Min(StartTick, targetTick - (_massive.CanRollbackFrames + 1) * _saveEachNthTick));

			_changeTracker.ConfirmChangesUpTo(targetTick);
		}
	}
}
