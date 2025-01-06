namespace Massive.Netcode
{
	public class Simulation
	{
		public MassiveRegistry Registry { get; }

		public SimulationGroup Systems { get; }

		public SimulationInputs Inputs { get; }

		public SimulationTime Time { get; }

		public ResimulationLoop Loop { get; }

		public ChangeTracker ChangeTracker { get; }

		public Simulation() : this(new SimulationConfig())
		{
		}

		public Simulation(SimulationConfig simulationConfig)
		{
			Registry = new MassiveRegistry(simulationConfig.MassiveRegistryConfig);

			Time = new SimulationTime(simulationConfig.Framerate);
			Inputs = new SimulationInputs(Time, Registry.Config.FramesCapacity * simulationConfig.SaveEachNthTick
				+ simulationConfig.AdditionalInputBufferSize);

			Systems = new SimulationGroup();
			Systems.Add(Time);

			ChangeTracker = new ChangeTracker();
			Inputs.InputChanged += ChangeTracker.NotifyChange;
			Loop = new ResimulationLoop(Registry, Systems, Inputs, ChangeTracker, simulationConfig.SaveEachNthTick);

			Registry.AssignService(Time);
			Registry.AssignService(Inputs);
		}
	}
}
