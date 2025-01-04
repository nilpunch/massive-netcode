namespace Massive.Netcode
{
	public class SimulationGroup : ISimulation
	{
		private readonly FastList<ISimulation> _systems = new FastList<ISimulation>();

		public void Add(ISimulation system)
		{
			_systems.Add(system);
		}

		public void Remove(ISimulation system)
		{
			_systems.Remove(system);
		}

		public void Update(int tick)
		{
			foreach (var simulationSystem in _systems)
			{
				simulationSystem.Update(tick);
			}
		}
	}
}
