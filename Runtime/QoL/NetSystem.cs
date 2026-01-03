namespace Massive.Netcode
{
	public class NetSystem : ISystem, ISystemInject<Session>
	{
		public int Id { get; private set; }
		public Session Session { get; private set; }

		public Inputs Inputs => Session.Inputs;

		public Time Time => Session.Time;

		public MassiveWorld World => Session.World;

		void ISystem.Build(int id, Allocator _)
		{
			Id = id;
		}

		void ISystemInject<Session>.Inject(Session session)
		{
			Session = session;
		}
	}
}
