namespace Massive.Netcode
{
	public class NetSystem : WorldSystem, ISystemInject<Session>
	{
		public Session Session { get; private set; }

		public Inputs Inputs => Session.Inputs;

		public Time Time => Session.Time;

		void ISystemInject<Session>.Inject(Session session)
		{
			Session = session;
		}
	}
}
