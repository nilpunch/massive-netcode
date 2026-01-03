namespace Massive.Netcode
{
	public static class NetSystemsExtensions
	{
		/// <summary>
		/// Builds systems and calls <see cref="ISystemInject{TArg}.Inject"/> with <see cref="Session"/> argument on all builded systems.
		/// </summary>
		public static Systems Build(this Systems systems, Session session)
		{
			return systems.Build().Inject(session);
		}
	}
}
