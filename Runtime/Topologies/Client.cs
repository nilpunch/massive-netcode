namespace Massive.Netcode
{
	public class Client
	{
		public Session Session { get; }

		public TickSync TickSync { get; }

		public Client(SessionConfig sessionConfig)
		{
			Session = new Session(sessionConfig);
			TickSync = new TickSync(sessionConfig.TickRate, sessionConfig.RollbackTicksCapacity);
		}

		public void Connect()
		{
		}

		public void Disconnect()
		{
		}

		public void Update(float currentTime)
		{
			TickSync.Update(currentTime);
			Session.Loop.FastForwardToTick(TickSync.TargetTick);
		}
	}
}
