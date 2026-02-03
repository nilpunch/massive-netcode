using System;

namespace Massive.Netcode
{
	public class Session
	{
		public SessionConfig Config { get; }

		public MassiveWorld World { get; }

		public MassiveSystems Systems { get; }

		public SimulationGroup Simulations { get; }

		public Inputs Inputs { get; }

		public ISimulationLoop Loop { get; }

		public ChangeTracker ChangeTracker { get; }

		public Session(IPredictionReceiver predictionReceiver = null) : this(new SessionConfig(), predictionReceiver)
		{
		}

		public Session(SessionConfig config, IPredictionReceiver predictionReceiver = null, bool resimulate = true)
		{
			Config = config;
			World = new MassiveWorld(config.WorldConfig);
			Systems = new MassiveSystems(config.FramesCapacity);
			Simulations = new SimulationGroup();
			ChangeTracker = new ChangeTracker();

			Inputs = new Inputs(ChangeTracker, predictionReceiver);
			Simulations.Add(Inputs);

			if (resimulate)
			{
				Loop = new ResimulationLoop(World, Simulations, Inputs, ChangeTracker, config.SaveEachNthTick);
			}
			else
			{
				Loop = new ForwardOnlyLoop(Simulations, Inputs, ChangeTracker);
			}
			World.FrameSaved += Systems.SaveFrame;
			World.Rollbacked += Systems.Rollback;
		}

		public void FastForwardToTime(double targetTime)
		{
			var targetTick = (int)Math.Floor(targetTime * Config.TickRate);
			Loop.FastForwardToTick(targetTick);
		}

		public void Reset(int startTick)
		{
			Inputs.Reset(startTick);
			Loop.Reset(startTick);
			ChangeTracker.NotifyChange(0);
			ChangeTracker.ConfirmChangesUpTo(startTick);
		}
	}
}
