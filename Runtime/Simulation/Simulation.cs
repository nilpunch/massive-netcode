namespace Massive.Netcode
{
	public class Simulation
	{
		public MassiveRegistry Registry { get; }

		public InputRegistry InputRegistry { get; }

		public SimulationGroup Group { get; }

		public SimulationInputs Inputs { get; }

		public SimulationTime Time { get; }

		public SimulationLoop Loop { get; }

		private TickChangeLog TickChangeLog { get; }

		public Simulation(int simulationFramerate = 60, int saveEachNthFrame = 5, MassiveRegistryConfig massiveRegistryConfig = null)
		{
			Registry = new MassiveRegistry(massiveRegistryConfig ?? new MassiveRegistryConfig());
			Time = new SimulationTime() { FPS = simulationFramerate };
			InputRegistry = new InputRegistry(Registry.Config.FramesCapacity * saveEachNthFrame, 0, new RegistryConfig(pageSize: 1024));
			Group = new SimulationGroup();
			TickChangeLog = new TickChangeLog();

			Inputs = new SimulationInputs(InputRegistry, Time);
			Group.Add(Time);

			InputRegistry.InputChanged += TickChangeLog.NotifyChange;

			Loop = new SimulationLoop(Registry, Group, InputRegistry, TickChangeLog, saveEachNthFrame);

			Registry.AssignService(Time);
			Registry.AssignService(Inputs);
		}
	}
}
