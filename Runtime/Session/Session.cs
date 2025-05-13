namespace Massive.Netcode
{
	public class Session
	{
		public MassiveWorld World { get; }

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
			World = new MassiveWorld(sessionConfig.WorldConfig);
			ChangeTracker = new ChangeTracker();

			Time = new Time(sessionConfig.Framerate);
			Inputs = new Inputs(Time, ChangeTracker, sessionConfig.StartTick);

			Simulations = new SimulationGroup();
			Simulations.Add(Time);

			Loop = new ResimulationLoop(World, Simulations, Inputs, ChangeTracker, sessionConfig.SaveEachNthTick);
		}
	}
}
