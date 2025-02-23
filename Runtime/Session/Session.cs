namespace Massive.Netcode
{
	public class Session
	{
		public MassiveRegistry Registry { get; }

		public ServiceLocator Services { get; }

		public SimulationGroup Simulations { get; }

		public Inputs Inputs { get; }

		public Time Time { get; }

		public ResimulationLoop Loop { get; }

		public ChangeTracker ChangeTracker { get; }

		public Session() : this(new SessionConfig())
		{
		}

		public Session(SessionConfig sessionConfig)
		{
			Registry = new MassiveRegistry(sessionConfig.RegistryConfig);
			ChangeTracker = new ChangeTracker();

			Time = new Time(sessionConfig.Framerate);
			Inputs = new Inputs(Time, ChangeTracker, sessionConfig.StartTick);

			Simulations = new SimulationGroup();
			Simulations.Add(Time);

			Loop = new ResimulationLoop(Registry, Simulations, Inputs, ChangeTracker, sessionConfig.SaveEachNthTick);

			Services = new ServiceLocator();
			Services.Assign<Registry>(Registry);
			Services.Assign(Time);
			Services.Assign(Inputs);
		}
	}
}
