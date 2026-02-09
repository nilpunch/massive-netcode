namespace Massive.Netcode
{
	public class BasicSimulation : ISimulation
	{
		private readonly Systems _systems;

		public BasicSimulation(Session session)
			: this(session.Systems)
		{
		}

		public BasicSimulation(Systems systems)
		{
			_systems = systems;
		}

		public void Update(int tick)
		{
			if (tick == 0)
			{
				_systems.Run<IFirstTick>();
			}

			_systems.Run<IUpdate>();
		}
	}
}
