namespace Massive.Netcode
{
	public class NetSystem : ISystem, IInject<Session>
	{
		public int Id { get; private set; }
		public Session Session { get; private set; }

		public Inputs Inputs => Session.Inputs;

		public int Tick => Session.Loop.CurrentTick;

		public MassiveWorld World => Session.World;

		void ISystem.Build(int id, Allocator _)
		{
			Id = id;
		}

		void IInject<Session>.Inject(Session session)
		{
			Session = session;
		}
	}
}
