using System.Collections.Generic;

namespace Massive.Netcode
{
	public class SimulationGroup : ISimulation
	{
		private readonly List<ISimulation> _systems = new List<ISimulation>();

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
