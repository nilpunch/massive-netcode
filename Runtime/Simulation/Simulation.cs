namespace Massive.Netcode
{
	public class Simulation
	{
		public MassiveRegistry Registry { get; }

		public SimulationGroup Group { get; }

		public SimulationInputs Inputs { get; }

		public SimulationTime Time { get; }

		public SimulationLoop Loop { get; }

		private TickChangeLog TickChangeLog { get; }

		public Simulation(int simulationFramerate = 60, int saveEachNthFrame = 5, MassiveRegistryConfig massiveRegistryConfig = null)
		{
			Registry = new MassiveRegistry(massiveRegistryConfig ?? new MassiveRegistryConfig());
			Time = new SimulationTime() { FPS = simulationFramerate };
			Inputs = new SimulationInputs(Time, Registry.Config.FramesCapacity * saveEachNthFrame, 0, new RegistryConfig(pageSize: 1024));
			Group = new SimulationGroup();
			TickChangeLog = new TickChangeLog();

			Group.Add(Time);

			Inputs.InputChanged += TickChangeLog.NotifyChange;

			Loop = new SimulationLoop(Registry, Group, Inputs, TickChangeLog, saveEachNthFrame);

			Registry.AssignService(Time);
			Registry.AssignService(Inputs);
		}
	}
}
