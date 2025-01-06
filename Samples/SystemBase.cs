namespace Massive.Netcode.Samples
{
	public abstract class SystemBase : ISimulation
	{
		public Simulation Simulation { get; }

		public SystemBase(Simulation simulation)
		{
			Simulation = simulation;
		}

		public abstract void Update(int tick);
	}
}
