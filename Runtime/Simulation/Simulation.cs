namespace Massive.Netcode
{
	public class Simulation
	{
		public MassiveRegistry Registry { get; }

		public SimulationSystemGroup Systems { get; }

		public SimulationInput Input { get; }

		public SimulationTime Time { get; }

		public ResimulationLoop Loop { get; }

		public ChangeTracker ChangeTracker { get; }

		public Simulation() : this(new SimulationConfig())
		{
		}

		public Simulation(SimulationConfig simulationConfig)
		{
			Registry = new MassiveRegistry(simulationConfig.RegistryConfig);

			Time = new SimulationTime(simulationConfig.Framerate);
			Input = new SimulationInput(Time, Registry.Config.FramesCapacity * simulationConfig.SaveEachNthTick
				+ simulationConfig.AdditionalInputBufferSize, simulationConfig.StartTick);

			Systems = new SimulationSystemGroup();
			Systems.Add(Time);

			ChangeTracker = new ChangeTracker();
			Input.InputChanged += ChangeTracker.NotifyChange;
			Loop = new ResimulationLoop(Registry, Systems, Input, ChangeTracker, simulationConfig.SaveEachNthTick);

			Registry.AssignService(Time);
			Registry.AssignService(Input);
		}
	}
}
