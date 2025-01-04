using System;

namespace Massive.Netcode
{
	public class ClientSimulation
	{
		
		
		public void NewPlayer()
		{
			
		}
	}
	
	public class Simulation
	{
		private readonly IMassive _massive;
		private readonly ISystem _system;
		private readonly IInputPrediction _inputPrediction;
		private readonly TickChangeLog _tickChangeLog;
		private readonly int _saveEachNthTick;

		public Simulation(IMassive massive, ISystem system, IInputPrediction inputPrediction, TickChangeLog tickChangeLog, int saveEachNthTick = 5)
		{
			_massive = massive;
			_system = system;
			_inputPrediction = inputPrediction;
			_tickChangeLog = tickChangeLog;
			_saveEachNthTick = saveEachNthTick;
		}

		private int CurrentTick { get; set; }

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
				_system.StepForward();
				CurrentTick += 1;

				if (CurrentTick % _saveEachNthTick == 0)
				{
					_massive.SaveFrame();
				}
			}

			_tickChangeLog.ConfirmUpTo(targetTick);
		}
	}
}
