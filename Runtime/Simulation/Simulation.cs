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

		public Simulation(int simulationFramerate = 60, int saveEachNthTick = 5, MassiveRegistryConfig massiveRegistryConfig = null)
		{
			Registry = new MassiveRegistry(massiveRegistryConfig ?? new MassiveRegistryConfig());

			Time = new SimulationTime(simulationFramerate);
			Inputs = new SimulationInputs(Time, Registry.Config.FramesCapacity * saveEachNthTick);

			Systems = new SimulationGroup();
			Systems.Add(Time);

			ChangeTracker = new ChangeTracker();
			Inputs.InputChanged += ChangeTracker.NotifyChange;
			Loop = new ResimulationLoop(Registry, Systems, Inputs, ChangeTracker, saveEachNthTick);

			Registry.AssignService(Time);
			Registry.AssignService(Inputs);
		}
	}
}
