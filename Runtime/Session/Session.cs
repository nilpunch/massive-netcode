namespace Massive.Netcode
{
	public class Session
	{
		public SessionConfig Config { get; }

		public MassiveWorld World { get; }

		public MassiveSystems Systems { get; }

		public SimulationGroup Simulations { get; }

		public Inputs Inputs { get; }

		public Time Time { get; }

		public ResimulationLoop Loop { get; }

		public ChangeTracker ChangeTracker { get; }

		public Session(IPredictionReceiver predictionReceiver = null) : this(new SessionConfig(), predictionReceiver)
		{
		}

		public Session(SessionConfig config, IPredictionReceiver predictionReceiver = null)
		{
			Config = config;
			World = new MassiveWorld(config.WorldConfig);
			Systems = new MassiveSystems(config.FramesCapacity);
			ChangeTracker = new ChangeTracker();

			Time = new Time(config.TickRate);
			Inputs = new Inputs(Time, ChangeTracker, config.StartTick, predictionReceiver);

			Simulations = new SimulationGroup();
			Simulations.Add(Time);

			Loop = new ResimulationLoop(World, Simulations, Inputs, ChangeTracker, config.SaveEachNthTick);
			World.FrameSaved += Systems.SaveFrame;
			World.Rollbacked += Systems.Rollback;
		}
	}
}
