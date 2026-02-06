using System;

namespace Massive.Netcode
{
	public class ForwardOnlyLoop : ISimulationLoop
	{
		private readonly ISimulation _simulation;
		private readonly IInputs _inputs;
		private readonly ChangeTracker _changeTracker;

		public ForwardOnlyLoop(ISimulation simulation, IInputs inputs, ChangeTracker changeTracker)
		{
			_simulation = simulation;
			_inputs = inputs;
			_changeTracker = changeTracker;
		}

		public int CurrentTick { get; private set; }

		public void Reset(int startTick)
		{
			CurrentTick = startTick;
		}

		public void FastForwardToTick(int targetTick)
		{
			if (targetTick < CurrentTick)
			{
				throw new ArgumentOutOfRangeException(nameof(targetTick), "Target tick should not be less than current.");
			}

			_inputs.Reevaluate();
			_inputs.PopulateUpTo(targetTick);

			while (CurrentTick < targetTick)
			{
				_simulation.Update(CurrentTick);
				CurrentTick += 1;
			}

			_changeTracker.ConfirmChangesUpTo(targetTick);
		}
	}
}
