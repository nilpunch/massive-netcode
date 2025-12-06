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

		public Session() : this(new SessionConfig())
		{
		}

		public Session(SessionConfig config)
		{
			Config = config;
			World = new MassiveWorld(config.WorldConfig);
			Systems = new MassiveSystems(config.FramesCapacity);
			ChangeTracker = new ChangeTracker();

			Time = new Time(config.Framerate);
			Inputs = new Inputs(Time, ChangeTracker, config.StartTick);

			Simulations = new SimulationGroup();
			Simulations.Add(Time);

			Loop = new ResimulationLoop(
				new MassiveGroup(config.FramesCapacity, World, Systems),
				Simulations, Inputs, ChangeTracker, config.SaveEachNthTick);
		}
	}
}
