namespace Massive.Netcode
{
	public class SessionSerialization
	{
		private readonly Session _session;
		private readonly InputIdentifiers _inputIdentifiers;

		public SessionSerialization(Session session, InputIdentifiers inputIdentifiers)
		{
			_session = session;
			_inputIdentifiers = inputIdentifiers;
		}
	}
}
