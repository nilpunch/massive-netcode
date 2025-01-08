namespace Massive.Netcode
{
	public class SimulationSystemGroup : ISimulationSystem
	{
		private readonly FastList<ISimulationSystem> _systems = new FastList<ISimulationSystem>();

		public void Add(ISimulationSystem system)
		{
			_systems.Add(system);
		}

		public void Remove(ISimulationSystem system)
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
